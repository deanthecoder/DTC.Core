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
using System.Threading.Tasks;
using Material.Icons;

namespace CSharp.Core.UI;

public interface IDialogService
{
    void Warn(string message, string detail, string cancelButton, string actionButton, Action<bool> onClose, MaterialIconKind? icon = null);
    void ShowMessage(string message, string detail, MaterialIconKind? icon = MaterialIconKind.Information);
    Task<FileInfo> ShowFileOpenAsync(string title, string filterName, string[] filterExtensions);
    Task<FileInfo> ShowFileSaveAsync(string title, string defaultFileName, string filterName, string[] filterExtensions);
    Task<DirectoryInfo> SelectFolderAsync(string title, DirectoryInfo defaultFolder = null);
}