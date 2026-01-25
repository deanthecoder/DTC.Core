// Code authored by Dean Edis (DeanTheCoder).
// Anyone is free to copy, modify, use, compile, or distribute this software,
// either in source code form or as a compiled binary, for any purpose.
//
// If you modify the code, please retain this copyright header,
// and consider contributing back to the repository or letting us know
// about your modifications. Your contributions are valued!
//
// THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND.

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using DTC.Core.Extensions;

namespace DTC.Core.Recording;

/// <summary>
/// Captures video frames from an Avalonia bitmap and encodes them into a compressed movie using FFmpeg.
/// </summary>
/// <remarks>
/// Typical usage:
/// <list type="number">
///   <item>
///     <description>Create a session with the target <see cref="WriteableBitmap"/> and its frame rate.</description>
///   </item>
///   <item>
///     <description>Call <see cref="Start"/> once before capturing frames.</description>
///   </item>
///   <item>
///     <description>Call <see cref="CaptureFrame"/> after each render (on the render thread).</description>
///   </item>
///   <item>
///     <description>Call <see cref="StopAsync"/> to finalize and then move the returned temp file.</description>
///   </item>
/// </list>
/// Audio capture is optional: pass a <see cref="RecordingAudioSettings"/> to enable WAV capture, then
/// feed PCM samples via <see cref="OnAudioSamples"/>. If audio is not provided,
/// the output is video-only.
/// The session is safe to call from render/update threads; internal locking keeps FFmpeg input consistent.
/// </remarks>
public sealed class RecordingSession : IDisposable
{
    private readonly WriteableBitmap m_display;
    private readonly double m_frameRate;
    private readonly RecordingAudioSettings m_audioSettings;
    private readonly TempDirectory m_tempDirectory;
    private readonly FileInfo m_tempVideoFile;
    private readonly FileInfo m_tempAudioFile;
    private readonly FileInfo m_tempOutputFile;
    private readonly object m_sync = new();
    private Process m_videoProcess;
    private Stream m_videoInput;
    private WavFileWriter m_audioWriter;
    private byte[] m_videoBuffer;
    private int m_expectedRowBytes;
    private int m_frameHeight;
    private int m_frameWidth;
    private int m_bytesPerPixel;
    private string m_inputPixelFormat;
    private Stopwatch m_stopwatch;
    private bool m_isDisposed;
    private volatile bool m_isRecording;

    public RecordingSession(WriteableBitmap display, double frameRate, RecordingAudioSettings audioSettings = null)
    {
        m_display = display ?? throw new ArgumentNullException(nameof(display));
        if (frameRate <= 0)
            throw new ArgumentOutOfRangeException(nameof(frameRate));

        m_frameRate = frameRate;
        m_audioSettings = audioSettings;
        m_tempDirectory = new TempDirectory();
        m_tempVideoFile = m_tempDirectory.GetFile("recording-video.mp4");
        m_tempAudioFile = m_tempDirectory.GetFile("recording-audio.wav");
        m_tempOutputFile = m_tempDirectory.GetFile("recording.mp4");
    }

    public bool IsRecording => m_isRecording;

    public static bool IsFfmpegAvailable(out string reason)
    {
        reason = string.Empty;
        var startInfo = new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = "-version"
        };

        try
        {
            var result = startInfo.RunAndCaptureOutput(TimeSpan.FromSeconds(2));
            if (result?.IsSuccess == true)
                return true;

            reason = string.IsNullOrWhiteSpace(result?.StandardError)
                ? "FFmpeg did not report a valid version."
                : result.StandardError;
            return false;
        }
        catch (Exception ex)
        {
            reason = ex.Message;
            return false;
        }
    }

    public void Start()
    {
        if (m_isRecording)
            return;

        var size = m_display.PixelSize;
        if (size.Width <= 0 || size.Height <= 0)
            throw new InvalidOperationException("Display size is invalid.");

        m_frameWidth = size.Width;
        m_frameHeight = size.Height;
        m_bytesPerPixel = 3;
        m_inputPixelFormat = "rgb24";
        if (m_display.Format != PixelFormats.Rgb24)
            throw new NotSupportedException($"Unsupported pixel format for recording: {m_display.Format}. RGB24 is required.");
        m_expectedRowBytes = m_frameWidth * m_bytesPerPixel;
        m_videoBuffer = new byte[m_expectedRowBytes * m_frameHeight];

        StartVideoProcess();
        if (m_audioSettings != null)
            m_audioWriter = new WavFileWriter(m_tempAudioFile, m_audioSettings.SampleRate, m_audioSettings.Channels);

        m_stopwatch = Stopwatch.StartNew();
        m_isRecording = true;
        Logger.Instance.Info("Recording started.");
    }

    public void CaptureFrame()
    {
        if (!m_isRecording)
            return;

        try
        {
            lock (m_sync)
            {
                if (!m_isRecording || m_videoInput == null)
                    return;

                WriteFrame(m_display);
            }
        }
        catch (Exception ex)
        {
            Logger.Instance.Warn($"Video capture failed: {ex.Message}");
            StopOnError();
        }
    }

    /// <summary>
    /// Placeholder hook for future audio capture.
    /// </summary>
    public void OnAudioSamples(ReadOnlySpan<short> samples, int sampleRate)
    {
        if (!m_isRecording || samples.IsEmpty)
            return;

        if (m_audioSettings == null)
            return;

        if (sampleRate != m_audioSettings.SampleRate)
            return;

        try
        {
            lock (m_sync)
                m_audioWriter?.WriteSamples(samples);
        }
        catch (Exception ex)
        {
            Logger.Instance.Warn($"Audio capture failed: {ex.Message}");
            StopOnError();
        }
    }

    public Task<RecordingResult> StopAsync(ProgressToken progress = null)
    {
        if (!m_isRecording)
            return Task.FromResult<RecordingResult>(null);

        m_isRecording = false;
        m_stopwatch?.Stop();

        return Task.Run(() => FinalizeRecording(progress));
    }

    public void Dispose()
    {
        if (m_isDisposed)
            return;

        m_isDisposed = true;
        if (m_isRecording)
        {
            try
            {
                StopAsync()?.Wait(TimeSpan.FromSeconds(10));
            }
            catch
            {
                // Continue.
            }
        }

        m_tempDirectory?.Dispose();
    }

    private void StartVideoProcess()
    {
        var args =
            $"-y -f rawvideo -pixel_format {m_inputPixelFormat} -video_size {m_frameWidth}x{m_frameHeight} " +
            $"-framerate {m_frameRate:0.####} -i - -an -c:v libx264 -preset veryfast -crf 18 -pix_fmt yuv420p " +
            $"\"{m_tempVideoFile.FullName}\"";

        var startInfo = new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = args,
            RedirectStandardInput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        m_videoProcess = new Process { StartInfo = startInfo, EnableRaisingEvents = false };
        if (!m_videoProcess.Start())
            throw new InvalidOperationException("Failed to start ffmpeg.");

        m_videoProcess.ErrorDataReceived += (_, _) => { };
        m_videoProcess.BeginErrorReadLine();
        m_videoInput = m_videoProcess.StandardInput.BaseStream;
    }

    private RecordingResult FinalizeRecording(ProgressToken progress)
    {
        lock (m_sync)
        {
            try
            {
                m_videoInput?.Flush();
                m_videoInput?.Dispose();
            }
            catch (Exception ex)
            {
                Logger.Instance.Warn($"Failed to finalize video stream: {ex.Message}");
            }

            m_videoInput = null;

            try
            {
                m_audioWriter?.Dispose();
            }
            catch (Exception ex)
            {
                Logger.Instance.Warn($"Failed to finalize audio stream: {ex.Message}");
            }

            m_audioWriter = null;
        }

        try
        {
            if (m_videoProcess != null)
            {
                var waitTimer = Stopwatch.StartNew();
                var lastLogMs = 0L;
                while (!m_videoProcess.HasExited)
                {
                    m_videoProcess.WaitForExit(1000);

                    if (waitTimer.ElapsedMilliseconds >= 10000 && waitTimer.ElapsedMilliseconds - lastLogMs >= 30000)
                    {
                        Logger.Instance.Info($"FFmpeg still encoding... ({waitTimer.Elapsed:hh\\:mm\\:ss} elapsed)");
                        lastLogMs = waitTimer.ElapsedMilliseconds;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Instance.Warn($"Failed waiting for FFmpeg to exit: {ex.Message}");
        }

        if (m_videoProcess != null && m_videoProcess.HasExited && m_videoProcess.ExitCode != 0)
            Logger.Instance.Warn($"FFmpeg video encoder exited with code {m_videoProcess.ExitCode}.");

        m_videoProcess?.Dispose();
        m_videoProcess = null;

        try
        {
            MuxAudioVideo(progress);
        }
        catch (Exception ex)
        {
            Logger.Instance.Warn($"Failed to finalize recording: {ex.Message}");
        }
        finally
        {
            CleanupIntermediateFiles();
        }

        return m_tempOutputFile.Exists ? new RecordingResult(m_tempOutputFile, m_stopwatch?.Elapsed ?? TimeSpan.Zero) : null;
    }

    private void MuxAudioVideo(ProgressToken progress)
    {
        if (!m_tempVideoFile.Exists)
            return;

        var hasAudio = m_audioSettings != null && m_tempAudioFile.Exists;
        var audioPath = hasAudio ? $" -i \"{m_tempAudioFile.FullName}\"" : string.Empty;
        var audioArgs = hasAudio
            ? $" -c:a aac -b:a {m_audioSettings.BitRateKbps}k -shortest"
            : " -an";

        var args = $"-y -i \"{m_tempVideoFile.FullName}\"{audioPath} -c:v copy{audioArgs} -progress pipe:1 -nostats \"{m_tempOutputFile.FullName}\"";

        var muxInfo = new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = args,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        var durationMs = Math.Max(0.0, (m_stopwatch?.Elapsed ?? TimeSpan.Zero).TotalMilliseconds);
        if (progress != null && durationMs > 0.0)
        {
            progress.IsIndeterminate = false;
            progress.Progress = 0;
        }

        using var process = new Process();
        process.StartInfo = muxInfo;
        process.EnableRaisingEvents = false;
        if (!process.Start())
        {
            Logger.Instance.Warn("Failed to start FFmpeg for muxing.");
            return;
        }

        var errorBuilder = new StringBuilder();
        process.ErrorDataReceived += (_, argsData) =>
        {
            if (!string.IsNullOrWhiteSpace(argsData.Data))
                errorBuilder.AppendLine(argsData.Data);
        };
        process.OutputDataReceived += (_, argsData) =>
        {
            if (argsData.Data == null)
                return;

            if (progress == null || durationMs <= 0.0)
                return;

            if (TryParseOutTimeMs(argsData.Data, out var outMs))
            {
                var pct = (int)Math.Clamp(outMs / durationMs * 100.0, 0.0, 99.0);
                progress.Progress = pct;
            }
            else if (argsData.Data.StartsWith("progress=end", StringComparison.OrdinalIgnoreCase))
            {
                progress.Progress = 100;
            }
        };

        process.BeginErrorReadLine();
        process.BeginOutputReadLine();
        process.WaitForExit();

        if (progress != null && durationMs > 0.0)
            progress.Progress = 100;

        if (process.ExitCode != 0)
        {
            var errorText = errorBuilder.ToString().Trim();
            Logger.Instance.Warn($"Failed to finalize recording: {(string.IsNullOrWhiteSpace(errorText) ? $"FFmpeg exited with code {process.ExitCode}." : errorText)}");
        }
    }

    private static bool TryParseOutTimeMs(string line, out double ms)
    {
        ms = 0.0;
        if (string.IsNullOrWhiteSpace(line))
            return false;

        if (line.StartsWith("out_time_ms=", StringComparison.OrdinalIgnoreCase))
        {
            if (long.TryParse(line["out_time_ms=".Length..], out var value))
            {
                ms = value / 1000.0;
                return true;
            }
        }

        if (line.StartsWith("out_time_us=", StringComparison.OrdinalIgnoreCase))
        {
            if (long.TryParse(line["out_time_us=".Length..], out var value))
            {
                ms = value / 1000.0;
                return true;
            }
        }

        if (line.StartsWith("out_time=", StringComparison.OrdinalIgnoreCase))
        {
            var value = line["out_time=".Length..];
            if (TimeSpan.TryParse(value, out var timeSpan))
            {
                ms = timeSpan.TotalMilliseconds;
                return true;
            }
        }

        return false;
    }

    private void CleanupIntermediateFiles()
    {
        m_tempVideoFile.TryDelete();
        m_tempAudioFile.TryDelete();
    }

    private void WriteFrame(WriteableBitmap bitmap)
    {
        using var locked = bitmap.Lock();
        var rowBytes = locked.RowBytes;
        var expectedStride = m_expectedRowBytes;
        var frameBytes = expectedStride * m_frameHeight;

        if (m_videoBuffer.Length != frameBytes)
            m_videoBuffer = new byte[frameBytes];

        if (rowBytes == expectedStride)
        {
            Marshal.Copy(locked.Address, m_videoBuffer, 0, frameBytes);
        }
        else
        {
            var destIndex = 0;
            for (var y = 0; y < m_frameHeight; y++)
            {
                var srcOffset = locked.Address + y * rowBytes;
                Marshal.Copy(srcOffset, m_videoBuffer, destIndex, expectedStride);
                destIndex += expectedStride;
            }
        }

        m_videoInput?.Write(m_videoBuffer, 0, frameBytes);
    }

    // RGB24-only to match emulator output.

    private void StopOnError()
    {
        if (!m_isRecording)
            return;

        _ = StopAsync();
    }

    public sealed class RecordingResult
    {
        public RecordingResult(FileInfo tempFile, TimeSpan duration)
        {
            TempFile = tempFile;
            Duration = duration;
        }

        public FileInfo TempFile { get; }
        public TimeSpan Duration { get; }
    }
}

public sealed class RecordingAudioSettings
{
    public RecordingAudioSettings(int sampleRate, short channels, int bitRateKbps = 192)
    {
        if (sampleRate <= 0)
            throw new ArgumentOutOfRangeException(nameof(sampleRate));
        if (channels <= 0)
            throw new ArgumentOutOfRangeException(nameof(channels));
        if (bitRateKbps <= 0)
            throw new ArgumentOutOfRangeException(nameof(bitRateKbps));

        SampleRate = sampleRate;
        Channels = channels;
        BitRateKbps = bitRateKbps;
    }

    public int SampleRate { get; }
    public short Channels { get; }
    public int BitRateKbps { get; }
}
