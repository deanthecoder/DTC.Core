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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace DTC.Core.Markdown;

public partial class ConsoleFormatter
{
    [Flags]
    public enum Style
    {
        None = 0,
        Bold = 1 << 0,
        Italic = 1 << 1,
        Strike = 1 << 2,
        Highlight = 1 << 3,
        Underline = 1 << 4,
        Dim =  1 << 5,
        DimBackground = 1 << 6
    }

    public enum StyleSupport
    {
        None,
        Basic,
        Full
    }
    
    // P/Invoke
    private const int STD_OUTPUT_HANDLE = -11;

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);
    
    private readonly ConsoleRenderer m_consoleRenderer;
    private readonly IReadOnlyDictionary<Style, (string Start, string End)> m_styleSequences;
    private const string EscapePrefix = "\u001b[";

    public StyleSupport Support { get; set; }

    // Bold formatting
    public string BoldStart => $"{EscapePrefix}1m";
    public string BoldEnd => $"{EscapePrefix}22m";

    // Italic formatting
    public string ItalicStart => $"{EscapePrefix}3m";
    public string ItalicEnd => $"{EscapePrefix}23m";
    
    // Strike-through formatting
    public string StrikeStart => Support == StyleSupport.Full ? $"{EscapePrefix}9m" : "--";
    public string StrikeEnd => Support == StyleSupport.Full ? $"{EscapePrefix}29m" : "--";

    // Cyan color formatting
    public string CyanStart => $"{EscapePrefix}36m";
    public string CyanEnd => $"{EscapePrefix}39m";
    
    // Dark gray color formatting
    public string DarkGrayStart => $"{EscapePrefix}90m";
    public string DarkGrayEnd => $"{EscapePrefix}39m";
    
    // Dark gray background formatting
    public string DimBackStart => $"{EscapePrefix}48;2;48;48;48m";
    public string DimBackEnd => $"{EscapePrefix}49m";

    // Underline formatting
    public string UnderlineStart => $"{EscapePrefix}4m";
    public string UnderlineEnd => $"{EscapePrefix}24m";

    public ConsoleFormatter(ConsoleRenderer consoleRenderer)
    {
        m_consoleRenderer = consoleRenderer ?? throw new ArgumentNullException(nameof(consoleRenderer));

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            if (Console.IsOutputRedirected || !EnableWindowsVt())
            {
                Support = StyleSupport.None;
            }
            else
            {
                Support = StyleSupport.Basic;
                
                if (HasAnyEnv(
                        "WT_SESSION", // Windows Terminal
                        ("TERM_PROGRAM", "vscode"),
                        ("ConEmuANSI", "ON"), // ConEmu
                        "ANSICON" // ANSICON shim
                    ))
                {
                    // Assume full support for known terminal environments.
                    Support = StyleSupport.Full;
                }
            }
        }
        else
        {
            // Assume full support for non-Windows platforms.
            Support = StyleSupport.Full;
        }

        m_styleSequences = new Dictionary<Style, (string Start, string End)>
        {
            { Style.Bold, (BoldStart, BoldEnd) },
            { Style.Italic, (ItalicStart, ItalicEnd) },
            { Style.Strike, (StrikeStart, StrikeEnd) },
            { Style.Underline, (UnderlineStart, UnderlineEnd) },
            { Style.Highlight, (CyanStart, CyanEnd) },
            { Style.Dim, (DarkGrayStart, DarkGrayEnd) },
            { Style.DimBackground, (DimBackStart, DimBackEnd) }
        };
    }
    
    private static bool HasAnyEnv(params object[] items)
    {
        // Allows mixing string and (name, equals) pairs
        foreach (var it in items)
        {
            if (it is string s)
            {
                if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable(s)))
                    return true;
            }
            else if (it is ValueTuple<string,string> p)
            {
                if (string.Equals(Environment.GetEnvironmentVariable(p.Item1), p.Item2, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
        }
        return false;
    }
    
    private static bool EnableWindowsVt()
    {
        try
        {
            var handle = GetStdHandle(STD_OUTPUT_HANDLE);
            if (handle == IntPtr.Zero || handle == new IntPtr(-1))
                return false;

            if (!GetConsoleMode(handle, out var mode))
                return false;

            const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;

            var desired = mode | ENABLE_VIRTUAL_TERMINAL_PROCESSING;
            return SetConsoleMode(handle, desired);
        }
        catch
        {
            return false;
        }
    }

    public void ApplyStyles(Style styles) =>
        WriteStyles(styles, enable: true);

    public void ResetStyles(Style styles) =>
        WriteStyles(styles, enable: false);

    private void WriteStyles(Style styles, bool enable)
    {
        if (styles == Style.None || Support == StyleSupport.None)
            return;

        var allStyles = Enum.GetValues<Style>();
        foreach (var style in enable ? allStyles : allStyles.Reverse())
        {
            if (!styles.HasFlag(style) || !m_styleSequences.TryGetValue(style, out var sequence))
                continue;

            var value = enable ? sequence.Start : sequence.End;
            if (!string.IsNullOrEmpty(value))
                m_consoleRenderer.Write(value);
        }
    }

    public static string StripControls(string text)
    {
        // Strip ANSI escape sequences.
        return CodeStripperRegex().Replace(text, string.Empty);
    }

    [GeneratedRegex(@"\u001b\[[0-9;]*[a-zA-Z]|\u001b\]8;;.*?\u001b\\")]
    private static partial Regex CodeStripperRegex();
}