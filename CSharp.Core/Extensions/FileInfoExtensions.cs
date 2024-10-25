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
        info == null ? null : new FileInfo(info.FullName);

    /// <summary>
    /// Copies the source FileInfo to the specified destination.
    /// </summary>
    /// <remarks>
    /// If the destination file exists (with the correct content) and fastCopy is enabled, the operation is skipped.
    /// </remarks>
    public static void CopyTo(this FileInfo source, FileInfo dest, bool fastCopy = false)
    {
        if (!source.Exists())
            return; // Nothing to do.
        if (!dest.Exists || (fastCopy && !source.AreFilesEqual(dest)))
            source.CopyTo(dest.FullName, overwrite: true);
    }
    
    /// <summary>
    /// Copies the source FileInfo to the specified destination.
    /// </summary>
    /// <remarks>
    /// If the destination file exists (with the correct content) and fastCopy is enabled, the operation is skipped.
    /// </remarks>
    public static void CopyTo(this FileInfo source, DirectoryInfo dest, bool fastCopy = false)
    {
        if (!source.Exists())
            return; // Nothing to do.
        if (!dest.Exists())
            dest.Create();

        source.CopyTo(dest.GetFile(source.Name), fastCopy);
    }
    
    /// <summary>
    /// Checks if two files are equal by comparing their length and last write time.
    /// </summary>
    public static bool AreFilesEqual(this FileInfo source, FileInfo dest) =>
        source.Length == dest.Length && source.LastWriteTime == dest.LastWriteTime;
}