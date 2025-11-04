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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace DTC.Core.Extensions;

public static class DirectoryInfoExtensions
{
    public static FileInfo GetFile(this DirectoryInfo info, string name) =>
        Path.Combine(info.FullName, name).ToFile();

    public static DirectoryInfo GetDir(this DirectoryInfo info, string name) =>
        Path.Combine(info.FullName, name).ToDir();

    /// <summary>
    /// Attempts to recursively delete the specified directory.
    /// </summary>
    /// <remarks>
    /// If an error occurs during deletion, it is caught and ignored without throwing an exception.
    /// </remarks>
    public static bool TryDelete(this DirectoryInfo info)
    {
        try
        {
            if (info.Exists())
                info.Delete(true);
        }
        catch
        {
            // This is ok.
        }

        return !info.Exists();
    }
    
    public static bool IsAccessible(this DirectoryInfo directory)
    {
        try
        {
            _ = directory.EnumerateFileSystemInfos("*", SearchOption.TopDirectoryOnly).FirstOrDefault();
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    public static DirectoryInfo[] TryGetDirs(this DirectoryInfo info, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
    {
        if (info?.Exists() != true || !info.IsAccessible())
            return [];

        var pending = new Queue<DirectoryInfo>();
        pending.Enqueue(info);

        var results = new List<DirectoryInfo>();
        while (pending.Count > 0)
        {
            var current = pending.Dequeue();
            foreach (var dir in current.EnumerateDirectories(searchPattern, SearchOption.TopDirectoryOnly).Where(o => o.IsAccessible()))
            {
                results.Add(dir);
                if (searchOption == SearchOption.AllDirectories)
                    pending.Enqueue(dir);
            }
        }

        return results.ToArray();
    }
    
    public static FileInfo[] TryGetFiles(this DirectoryInfo info, string searchPattern = "*.*", SearchOption searchOption = SearchOption.TopDirectoryOnly) =>
        info
            .TryGetDirs(searchOption: searchOption)
            .Union([info])
            .SelectMany(o => o.GetFiles(searchPattern))
            .ToArray();

    public static FileSystemInfo[] TryGetContent(this DirectoryInfo info, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
    {
        try
        {
            if (info?.Exists() == true && info.IsAccessible())
                return info.GetFileSystemInfos(searchPattern, searchOption);
        }
        catch
        {
            // This is ok.
        }
        return [];
    }

    public static async IAsyncEnumerable<FileSystemInfo> TryGetContentAsync(
        this DirectoryInfo info,
        string fileSearchPattern = "*",
        SearchOption searchOption = SearchOption.TopDirectoryOnly,
        Func<bool> isCancelRequested = null)
    {
        if (isCancelRequested?.Invoke() ?? false)
            yield break;

        // Skip if the directory is a symlink or doesn't exist.
        if (info?.Exists() != true || info.IsSymbolicLink() || !info.IsAccessible())
            yield break;

        // Offload file enumeration to a background thread
        FileSystemInfo[] entries;
        try
        {
            entries = await Task.Run(() => info.GetFileSystemInfos(fileSearchPattern));
        }
        catch
        {
            yield break; // Skip directories we cannot access
        }

        foreach (var entry in entries)
            yield return entry;

        if (searchOption == SearchOption.TopDirectoryOnly)
            yield break;

        // Recursively process subdirectories
        IEnumerable<DirectoryInfo> subDirectories;
        try
        {
            subDirectories = info.GetDirectories().Where(o => !o.IsSymbolicLink()).ToArray();
        }
        catch
        {
            yield break; // Skip inaccessible directories
        }

        foreach (var subDirectory in subDirectories)
        {
            await foreach (var subEntry in subDirectory.TryGetContentAsync(fileSearchPattern, searchOption, isCancelRequested))
                yield return subEntry;
        }
    }
    
    /// <summary>
    /// Opens the directory in the OS's file explorer.
    /// </summary>
    public static void Explore(this DirectoryInfo directoryInfo)
    {
        var toOpen = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "explorer.exe" : "open";
        Process.Start(toOpen, directoryInfo.FullName);
    }

    public static DirectoryInfo Clone(this DirectoryInfo info) =>
        info?.FullName.ToDir();

    /// <summary>
    /// Resolves a path relative to the current DirectoryInfo (and maybe including file wildcard).
    /// </summary>
    public static string Resolve(this DirectoryInfo info, string morePath)
    {
        info.Resolve(morePath, out var dir, out var fileName, out var wildcard);

        var result = dir.FullName;
        if (!string.IsNullOrEmpty(fileName))
            result = Path.Combine(result, fileName);
        if (!string.IsNullOrEmpty(wildcard))
            result = Path.Combine(result, wildcard);
        return result;
    }

    /// <summary>
    /// Resolves a relative path within the current working directory to an absolute directory path, file name, and wildcard pattern.
    /// </summary>
    /// <param name="cwd">The current working directory.</param>
    /// <param name="relative">The relative path to resolve.</param>
    /// <param name="dir">The resolved absolute directory path.</param>
    /// <param name="fileName">The file name extracted from the path, if any.</param>
    /// <param name="wildcard">The wildcard pattern extracted from the path, if any.</param>
    public static void Resolve(this DirectoryInfo cwd, string relative, out DirectoryInfo dir, out string fileName, out string wildcard)
    {
        dir = cwd.Clone();

        if (relative.Length >= 3 && relative[1..3] == ":\\")
        {
            // Windows drive specified. Set the cwd to the drive root.
            dir = relative[..3].ToDir();
            relative = relative[2..];
        }

        if (string.IsNullOrEmpty(relative))
        {
            fileName = null;
            wildcard = null;
            return;
        }
        
        if (relative.IndexOfAny(['/', '\\']) == 0)
        {
            // Path is absolute.
            while (dir.Parent != null)
                dir = dir.Parent;
        }

        var parts = relative.Split(['\\', '/'], StringSplitOptions.RemoveEmptyEntries);
        for (var i = 0; i < parts.Length; i++)
        {
            var part = parts[i];
            if (part.IndexOfAny(['*', '?']) >= 0)
            {
                // We have a wildcard - Stop.
                wildcard = part;
                fileName = null;
                return;
            }

            if (part == "~")
            {
                // User's 'home' folder.
                dir = dir.GetDir(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
                continue;
            }

            if (part == "..")
            {
                // Navigate to parent.
                if (dir != null)
                    dir = dir.Parent;
                continue;
            }

            if (part == ".")
            {
                // Do nothing.
                continue;
            }

            // File or folder name containing a '.'
            if (part.Contains('.') && part.IndexOf('.') > 0)
            {
                if (i == parts.Length - 1)
                {
                    // This is the last section of the path.
                    // Check if it's a directory...
                    if (dir.GetDir(part).Exists())
                    {
                        // We know it's a directory containing a '.'
                        dir = dir.GetDir(part);
                        fileName = null;
                        wildcard = null;
                        return;
                    }
                    
                    // Can't be sure it's a directory, so assume a file.
                    fileName = part;
                    wildcard = null;
                    return;
                }
                
                // There's more path sections to come, so this must be a folder name with a '.'
            }

            // Normal folder name.
            dir = dir.GetDir(part);
        }

        fileName = null;
        wildcard = null;
    }

    /// <summary>
    /// Copies the contents of the current directory to the target directory recursively.
    /// </summary>
    /// <param name="source">The source directory to copy from.</param>
    /// <param name="target">The target directory to copy to.</param>
    /// <param name="fastCopy">Optional. Set to true to perform a fast copy (skipping if target files already exist in the correct state).</param>
    /// <returns>The number of files copied.</returns>
    public static int CopyTo(this DirectoryInfo source, DirectoryInfo target, bool fastCopy = false)
    {
        // Ensure source directory exists
        if (!source.Exists())
            return 0; // Nothing to copy.

        // Create the target directory with the same name as the source
        var targetSubDir = target.GetDir(source.Name);
        if (!targetSubDir.Exists)
            targetSubDir.Create();

        // Copy all files from the source to the target
        var filesCopied = 0;
        foreach (var file in source.GetFiles())
        {
            var targetFilePath = targetSubDir.GetFile(file.Name);
            filesCopied += file.CopyTo(targetFilePath, fastCopy) ? 1 : 0;
        }

        // Recursively copy subdirectories
        foreach (var subDir in source.GetDirectories())
            filesCopied += subDir.CopyTo(targetSubDir, fastCopy);

        return filesCopied;
    }


    /// <summary>
    /// Determines if the given directory is a symbolic link.
    /// </summary>
    public static bool IsSymbolicLink(this DirectoryInfo directory)
    {
        try
        {
            return (directory.Attributes & FileAttributes.ReparsePoint) != 0;
        }
        catch
        {
            // If we can't access attributes, assume it's not a symlink.
            return false;
        }
    }
}
