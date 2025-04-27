// Code authored by Dean Edis (DeanTheCoder).
// Anyone is free to copy, modify, use, compile, or distribute this software,
// either in source code form or as a compiled binary, for any non-commercial
//  purpose.
// 
// If you modify the code, please retain this copyright header,
// and consider contributing back to the repository or letting us know
// about your modifications. Your contributions are valued!
// 
// THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND.
using System;
using System.IO;

namespace CSharp.Core.Extensions;

public static class FileSystemInfoExtensions
{
    public static void TryDelete(this FileSystemInfo info)
    {
        if (info is FileInfo fileInfo)
            fileInfo.TryDelete();
        else if (info is DirectoryInfo directoryInfo)
            directoryInfo.TryDelete();
    }
    
    /// <summary>
    /// Copy the source object to the target folder.
    /// </summary>
    /// <returns>The number of files copied.</returns>
    public static int CopyTo(this FileSystemInfo source, DirectoryInfo dest, bool fastCopy = false)
    {
        if (source is FileInfo fileInfo)
            return fileInfo.CopyTo(dest, fastCopy) ? 1 : 0;
        if (source is DirectoryInfo directoryInfo)
            return directoryInfo.CopyTo(dest, fastCopy);

        throw new ArgumentException($"Unsupported file type: {source}");
    }
}