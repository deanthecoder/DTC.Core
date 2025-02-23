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

using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CSharp.Core.Extensions;

public static class FileInfoExtensions
{
    public static string LeafName(this FileInfo file)
    {
        var s = file?.Name ?? string.Empty;
        return string.IsNullOrEmpty(s) ? s : Path.GetFileNameWithoutExtension(s);
    }
    
    public static byte[] ReadAllBytes(this FileInfo file) =>
        file.Exists() ? File.ReadAllBytes(file.FullName) : null;

    public static string ReadAllText(this FileInfo file) =>
        file.Exists() ? File.ReadAllText(file.FullName) : null;

    public static string[] ReadAllLines(this FileInfo file) =>
        file.Exists() ? File.ReadAllLines(file.FullName) : null;

    public static FileInfo WriteAllText(this FileInfo file, string s)
    {
        File.WriteAllText(file.FullName, s);
        return file;
    }
    
    public static FileInfo WriteAllBytes(this FileInfo file, byte[] bytes)
    {
        File.WriteAllBytes(file.FullName, bytes);
        return file;
    }

    public static bool Exists(this FileSystemInfo info)
    {
        info.Refresh();
        return info.Exists;
    }

    public static bool TryDelete(this FileInfo file)
    {
        try
        {
            if (file.Exists())
                file.Delete();
        }
        catch
        {
            // This is ok.
        }

        return !File.Exists(file.FullName);
    }

    /// <summary>
    /// Opens the OS's file explorer and selects the specified file.
    /// </summary>
    public static void Explore(this FileInfo info)
    {
        if (info?.Exists() != true)
            return;
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            Process.Start("explorer.exe", $"/select,\"{info.FullName}\"");
        else
            Process.Start("open", $"-R \"{info.FullName}\"");
    }

    public static bool IsExecutable(this FileInfo info)
    {
        if (info?.Exists() != true)
            return false;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var extension = info.Extension.ToLowerInvariant();
            return extension is ".exe" or ".com" or ".msi" or ".bat";
        }
        
        // On macOS or Linux, check if the file has execute permissions.
        return (info.UnixFileMode & (UnixFileMode.UserExecute | UnixFileMode.GroupExecute | UnixFileMode.OtherExecute)) != 0;
    }

    public static FileInfo Clone(this FileInfo info) =>
        info?.FullName.ToFile();

    /// <summary>
    /// Copies the source FileInfo to the specified destination.
    /// </summary>
    /// <remarks>
    /// If the destination file exists (with the correct content) and fastCopy is enabled, the operation is skipped.
    /// </remarks>
    /// <returns>true if the file was copied.</returns>
    public static bool CopyTo(this FileInfo source, FileInfo dest, bool fastCopy = false)
    {
        if (!source.Exists())
            return false; // Nothing to copy.
        if (dest.Exists && fastCopy && source.AreFilesEqual(dest))
            return false; // No need to copy.
        source.CopyTo(dest.FullName, overwrite: true);
        return true;
    }
    
    /// <summary>
    /// Copies the source FileInfo to the specified destination.
    /// </summary>
    /// <remarks>
    /// If the destination file exists (with the correct content) and fastCopy is enabled, the operation is skipped.
    /// </remarks>
    /// <returns>true if the file was copied.</returns>
    public static bool CopyTo(this FileInfo source, DirectoryInfo dest, bool fastCopy = false)
    {
        if (!source.Exists())
            return false; // Nothing to copy.
        if (!dest.Exists())
            dest.Create();
        return source.CopyTo(dest.GetFile(source.Name), fastCopy);
    }
    
    /// <summary>
    /// Checks if two files are equal by comparing their length and last write time.
    /// </summary>
    public static bool AreFilesEqual(this FileInfo source, FileInfo dest) =>
        source.Length == dest.Length && source.LastWriteTime == dest.LastWriteTime;

    /// <summary>
    /// Try to deduce whether the file is (mostly) text-based.
    /// </summary>
    public static bool IsTextFile(this FileInfo file)
    {
        if (!file.Exists)
            throw new FileNotFoundException(file.FullName);
        if (file.Length == 0)
            return false; // Nothing to check for.
        
        // Handle common file extensions.
        var extensions = ".axaml,.bat,.c,.cpp,.cs,.csv,.csproj,.dotsettings,.gitignore,.glsl,.htm,.html,.ini,.iss,.json,.md,.ps1,.pubxml,.resx,.sh,.sksl,.sln,.txt,.user,.vcxproj,.xaml,.xml.h".Split(',');
        if (extensions.Contains(file.Extension.ToLowerInvariant()))
            return true;

        // Read a sample of the file.
        using var stream = file.OpenRead();
        var buffer = new byte[(int)Math.Min(4096L, file.Length)];
        var byteCount = stream.Read(buffer);
        if (byteCount < 2)
            return false;

        // Check for common BOMs.
        if ((byteCount >= 3 && buffer[0] == 0xEF && buffer[1] == 0xBB && buffer[2] == 0xBF) ||  // UTF-8 BOM
            (buffer[0] == 0xFF && buffer[1] == 0xFE) ||  // UTF-16 LE BOM
            (buffer[0] == 0xFE && buffer[1] == 0xFF))    // UTF-16 BE BOM
        {
            return true;
        }

        var printableCount = 0;
        for (var i = 0; i < byteCount; i++)
        {
            var b = buffer[i];
            if (b >= 32 && b <= 126)  // Printable ASCII (letters, numbers, symbols)
                printableCount++;
            else if (b == 9 || b == 10 || b == 13) // Tabs, newlines (valid in text)
                printableCount++;
        }

        // If at least 90% of the file is printable characters, it's text.
        return printableCount >= byteCount * 0.90;
    }
}