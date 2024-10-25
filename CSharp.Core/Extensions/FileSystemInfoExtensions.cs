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
    
    public static void CopyTo(this FileSystemInfo source, DirectoryInfo dest, bool fastCopy = false)
    {
        if (source is FileInfo fileInfo)
            fileInfo.CopyTo(dest, fastCopy);
        else if (source is DirectoryInfo directoryInfo)
            directoryInfo.CopyTo(dest, fastCopy);
    }
}