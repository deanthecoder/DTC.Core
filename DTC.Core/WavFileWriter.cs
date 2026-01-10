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
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace DTC.Core;

/// <summary>
/// Minimal PCM WAV writer (16-bit stereo).
/// </summary>
public sealed class WavFileWriter : IDisposable
{
    private const short BitsPerSample = 16;
    private readonly FileStream m_stream;
    private readonly int m_sampleRate;
    private readonly short m_channels;
    private long m_dataLength;
    private bool m_isDisposed;

    /// <summary>
    /// Creates a new WAV file and writes the initial header.
    /// </summary>
    public WavFileWriter(FileInfo file, int sampleRate, short channels)
    {
        if (file == null)
            throw new ArgumentNullException(nameof(file));

        m_sampleRate = sampleRate;
        m_channels = channels;
        m_stream = new FileStream(file.FullName, FileMode.Create, FileAccess.Write, FileShare.Read);
        WriteHeader();
    }

    /// <summary>
    /// Appends PCM samples to the WAV data section.
    /// </summary>
    public void WriteSamples(ReadOnlySpan<short> samples)
    {
        if (m_isDisposed || samples.IsEmpty)
            return;

        if (!BitConverter.IsLittleEndian)
        {
            Span<byte> temp = stackalloc byte[samples.Length * sizeof(short)];
            for (var i = 0; i < samples.Length; i++)
            {
                var value = samples[i];
                temp[i * 2] = (byte)(value & 0xFF);
                temp[i * 2 + 1] = (byte)((value >> 8) & 0xFF);
            }

            m_stream.Write(temp);
            m_dataLength += temp.Length;
            return;
        }

        var bytes = MemoryMarshal.AsBytes(samples);
        m_stream.Write(bytes);
        m_dataLength += bytes.Length;
    }

    /// <summary>
    /// Finalizes the WAV file and updates the header with correct sizes.
    /// </summary>
    public void Dispose()
    {
        if (m_isDisposed)
            return;

        m_isDisposed = true;
        UpdateHeader();
        m_stream.Dispose();
    }

    private void WriteHeader()
    {
        using var writer = new BinaryWriter(m_stream, Encoding.ASCII, leaveOpen: true);
        writer.Write(Encoding.ASCII.GetBytes("RIFF"));
        writer.Write(0); // Placeholder for file size.
        writer.Write(Encoding.ASCII.GetBytes("WAVE"));
        writer.Write(Encoding.ASCII.GetBytes("fmt "));
        writer.Write(16); // PCM header size.
        writer.Write((short)1); // PCM format.
        writer.Write(m_channels);
        writer.Write(m_sampleRate);
        writer.Write(m_sampleRate * m_channels * (BitsPerSample / 8));
        writer.Write((short)(m_channels * (BitsPerSample / 8)));
        writer.Write(BitsPerSample);
        writer.Write(Encoding.ASCII.GetBytes("data"));
        writer.Write(0); // Placeholder for data size.
    }

    private void UpdateHeader()
    {
        using var writer = new BinaryWriter(m_stream, Encoding.ASCII, leaveOpen: true);
        m_stream.Seek(4, SeekOrigin.Begin);
        writer.Write((int)Math.Min(int.MaxValue, 36 + m_dataLength));
        m_stream.Seek(40, SeekOrigin.Begin);
        writer.Write((int)Math.Min(int.MaxValue, m_dataLength));
    }
}
