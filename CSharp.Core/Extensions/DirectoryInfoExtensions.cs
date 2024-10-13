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

public static class DirectoryInfoExtensions
{
    public static FileInfo GetFile(this DirectoryInfo info, string name) =>
        new FileInfo(Path.Combine(info.FullName, name));

    public static DirectoryInfo GetDir(this DirectoryInfo info, string name) =>
        new DirectoryInfo(Path.Combine(info.FullName, name));

    public static bool TryDelete(this DirectoryInfo info)
    {
        try
        {
            info.Delete(true);
        }
        catch
        {
            // This is ok.
        }

        return !info.Exists();
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
        info == null ? null : new DirectoryInfo(info.FullName);

    /// <summary>
    /// Resolves a path relative to the current DirectoryInfo (and maybe including file wildcard).
    /// </summary>
    public static string Resolve(this DirectoryInfo info, string morePath)
    {
        if (string.IsNullOrEmpty(morePath))
            return info.FullName;
        
        if (morePath.StartsWith('~'))
        {
            info = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile).ToDir();
            morePath = morePath[1..].TrimStart('\\', '/');
        }

        while (morePath.StartsWith("..") && info.Parent != null)
        {
            info = info.Parent;
            morePath = morePath[2..].TrimStart('\\', '/');
        }
        
        // Remove any file filter.
        string result;
        var starIndex = morePath.IndexOf('*');
        if (starIndex >= 0)
        {
            result = Path.GetFullPath(Path.Combine(info.FullName, morePath.Substring(0, starIndex)));
            result = Path.Combine(result, morePath.Substring(starIndex));
        }
        else
        {
            result = Path.GetFullPath(Path.Combine(info.FullName, morePath));
        }
        
        return result;
    }
}
