// Code authored by Dean Edis (DeanTheCoder).
// Anyone is free to copy, modify, use, compile, or distribute this software,
// either in source code form or as a compiled binary, for any
// purpose.
//
// If you modify the code, please retain this copyright header,
// and consider contributing back to the repository or letting us know
// about your modifications. Your contributions are valued!
//
// THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND.

using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace DTC.Core.Extensions;

/// <summary>
/// Utilities for starting external processes and capturing their output.
/// </summary>
public static class ProcessExtensions
{
    /// <summary>
    /// Runs the supplied process start info and captures both stdout and stderr streams.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="startInfo"/> is null.</exception>
    /// <exception cref="TimeoutException">Thrown when the process fails to exit within <paramref name="timeout"/>.</exception>
    public static ProcessCaptureResult? RunAndCaptureOutput(this ProcessStartInfo startInfo, TimeSpan? timeout = null)
    {
        ArgumentNullException.ThrowIfNull(startInfo);

        startInfo.RedirectStandardOutput = true;
        startInfo.RedirectStandardError = true;
        startInfo.UseShellExecute = false;
        startInfo.CreateNoWindow = true;

        var standardOutputBuilder = new StringBuilder();
        var standardErrorBuilder = new StringBuilder();
        using var process = new Process();
        process.StartInfo = startInfo;
        
        try
        {
            if (!process.Start())
                return null;
        }
        catch
        {
            return null;
        }

        var outputCompleted = new TaskCompletionSource<bool>();
        var errorCompleted = new TaskCompletionSource<bool>();

        process.OutputDataReceived += (_, args) =>
        {
            if (args.Data is null)
            {
                outputCompleted.TrySetResult(true);
                return;
            }

            standardOutputBuilder.AppendLine(args.Data);
        };

        process.ErrorDataReceived += (_, args) =>
        {
            if (args.Data is null)
            {
                errorCompleted.TrySetResult(true);
                return;
            }

            standardErrorBuilder.AppendLine(args.Data);
        };

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        if (timeout.HasValue)
        {
            if (!process.WaitForExit((int)timeout.Value.TotalMilliseconds))
            {
                try
                {
                    process.Kill(entireProcessTree: true);
                }
                catch (Exception)
                {
                    // Swallow exceptions triggered by attempting to kill an already-terminated process.
                }

                throw new TimeoutException($"The process '{startInfo.FileName}' timed out after {timeout.Value.TotalSeconds:0.##} seconds.");
            }
        }
        else
        {
            process.WaitForExit();
        }

        Task.WaitAll(outputCompleted.Task, errorCompleted.Task);

        return new ProcessCaptureResult(process.ExitCode, standardOutputBuilder.ToString(), standardErrorBuilder.ToString());
    }
}

/// <summary>
/// Represents the outcome of running an external process.
/// </summary>
public sealed class ProcessCaptureResult
{
    public ProcessCaptureResult(int exitCode, string standardOutput, string standardError)
    {
        ExitCode = exitCode;
        StandardOutput = standardOutput?.TrimEnd() ?? string.Empty;
        StandardError = standardError?.TrimEnd() ?? string.Empty;
    }

    /// <summary>
    /// The exit code returned by the process.
    /// </summary>
    public int ExitCode { get; }

    /// <summary>
    /// Captured standard output.
    /// </summary>
    public string StandardOutput { get; }

    /// <summary>
    /// Captured standard error output.
    /// </summary>
    public string StandardError { get; }

    /// <summary>
    /// Indicates whether the process exited successfully (exit code 0).
    /// </summary>
    public bool IsSuccess => ExitCode == 0;
}
