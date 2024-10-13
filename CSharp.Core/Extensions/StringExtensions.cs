// Code authored by Dean Edis (DeanTheCoder).
// Anyone is free to copy, modify, use, compile, or distribute this software,
// either in source code form or as a compiled binary, for any non-commercial
// purpose.
//
// If you modify the code, please retain this copyright header,
// and consider contributing back to the repository or letting us know
// about your modifications. Your contributions are valued!
//
// THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND.

using System.Text.RegularExpressions;

namespace CSharp.Core.Extensions;

public static class StringExtensions
{
    public static string ToSafeFileName(this string s)
    {
        var badChars = Path.GetInvalidFileNameChars().Union(new[]
        {
            '\\', '/'
        });
        return badChars.Aggregate(s, (current, nameChar) => current.Replace(nameChar, '_'));
    }
    
    /// <summary>
    /// Add quotes to a file name string, if required.
    /// </summary>
    public static string AsFileName(this string s) =>
        string.IsNullOrEmpty(s) || !s.Contains(' ') ? s : $"\"{s.Trim('"')}\"";

    public static IEnumerable<string> ReadAllLines(this string s)
    {
        using var reader = new StringReader(s);
        while (reader.ReadLine() is { } line)
            yield return line;
    }

    public static DirectoryInfo ToDir(this string s) => new DirectoryInfo(s);
    public static FileInfo ToFile(this string s) => new FileInfo(s);

    private static readonly Regex CmdParsingRegex = new Regex(@"[\""].+?[\""]|[^ ]+", RegexOptions.Compiled);

    /// <summary>
    /// Parse a command string into an array of arguments by splitting based on spaces and preserving quoted segments.
    /// </summary>
    public static string[] ToArgumentArray(this string command) =>
        CmdParsingRegex.Matches(command).Select(o => o.Value.Trim('"')).ToArray();

    public static string StringOrDefault(this string s, string fallback) =>
        string.IsNullOrEmpty(s) ? fallback : s;
}
