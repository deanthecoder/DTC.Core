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

using System.IO;
using Avalonia.Platform.Storage;

namespace DTC.Core.Extensions;

public static class StorageItemExtensions
{
    public static FileInfo ToFileInfo(this IStorageFile storageFile) =>
        storageFile?.Path.LocalPath.ToFile();

    public static DirectoryInfo ToDirectoryInfo(this IStorageFolder storageFolder) =>
        storageFolder?.Path.LocalPath.ToDir();
}