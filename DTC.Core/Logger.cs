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
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using DTC.Core.Extensions;

namespace DTC.Core;

public class Logger
{
    private readonly FileInfo m_filePath = Assembly.GetEntryAssembly().GetAppSettingsPath().GetFile("log.txt");
    private readonly object m_lock = new();
    
    public enum Severity
    {
        Info,
        Warning,
        Error
    }
    
    public static Logger Instance { get; } = new Logger();

    public event EventHandler<(Severity, string Message)> Logged;

    public FileInfo File => m_filePath.Clone();

    private Logger()
    {
        try
        {
            if (m_filePath.Exists)
                m_filePath.TryDelete();
        }
        catch (Exception)
        {
            // Do nothing.
        }

        Logged += (_, info) =>
        {
            try
            {
                using var fileStream = m_filePath.Open(FileMode.Append);
                using var streamWriter = new StreamWriter(fileStream);
                streamWriter.WriteLine($"[{DateTime.Now:G}] {info.Item1}: {info.Message}");
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.Write("Error:");
                Console.ResetColor();
                Console.WriteLine($" Can't write to log file. ({e.Message})");
            }
        };
    }

    /// <summary>
    /// Log easily available system info and launch arguments.
    /// </summary>
    public void SysInfo()
    {
        var osName = OperatingSystem.IsWindows() ? "Windows" : OperatingSystem.IsMacOS() ? "Mac" : "Linux";
        Info($"OS: {osName} ({Environment.OSVersion})");
        Info($"Assembly: {Assembly.GetEntryAssembly()?.GetName().Name ?? "<Unknown>"}");
        Info($"Arguments: {Environment.CommandLine}");
        Info($"Locale: {CultureInfo.CurrentCulture}");
        Info($"Processors: {Environment.ProcessorCount}");
    }

    public void Info(Func<string> message) => Info(message());
    public void Warn(Func<string> message) => Warn(message());
    public void Error(Func<string> message) => Error(message());

    public void Info(string message)
    {
        lock (m_lock)
        {
            LogMessage(ConsoleColor.White, "Info: ", message);
            Logged?.Invoke(this, (Severity.Info, message));
        }
    }
    
    public void Warn(string message)
    {
        lock (m_lock)
        {
            LogMessage(ConsoleColor.DarkYellow, "Warn: ", message);
            Logged?.Invoke(this, (Severity.Warning, message));
        }
    }
    
    public void Error(string message)
    {
        lock (m_lock)
        {
            LogMessage(ConsoleColor.DarkRed, "Error: ", message);
            Logged?.Invoke(this, (Severity.Error, message));
        }
    }

    public void Exception(string message, Exception exception, bool includeInnerExceptions = true)
    {
        var detailedMessage = new StringBuilder();
        detailedMessage.AppendLine(message)
            .AppendLine($"Exception: {exception.Message}")
            .AppendLine($"Type: {exception.GetType()}")
            .AppendLine("StackTrace:")
            .AppendLine(exception.StackTrace);

        // Traverse inner exceptions if configured to do so
        if (includeInnerExceptions)
        {
            var currentInner = exception.InnerException;
            var depth = 1;
            while (currentInner is not null)
            {
                detailedMessage.AppendLine($"--- Inner Exception Level {depth} ---")
                    .AppendLine($"Message: {currentInner.Message}")
                    .AppendLine($"Type: {currentInner.GetType()}")
                    .AppendLine("StackTrace:")
                    .AppendLine(currentInner.StackTrace);

                currentInner = currentInner.InnerException;
                depth++;
            }
        }

        Error(detailedMessage.ToString());
    }
    
    private static void LogMessage(ConsoleColor color, string prefix, string message)
    {
        Console.ForegroundColor = color;
        Console.Write(prefix);
        Console.ResetColor();
        Console.WriteLine(message);
    }
}
