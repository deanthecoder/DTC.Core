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

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using CSharp.Core.Extensions;
using DialogHostAvalonia;
using Material.Icons;

namespace CSharp.Core.UI;

public class DialogService : IDialogService
{
    public static IDialogService Instance { get; } = new DialogService();

    public void Warn(string message, string detail, string cancelButton, string actionButton, Action<bool> onClose, MaterialIconKind? icon = null)
    {
        DialogHost.Show(new ConfirmationDialog
            {
                Message = message, 
                Detail = detail,
                CancelButton = cancelButton,
                ActionButton = actionButton,
                ActionBrush = Brushes.Red,
                Icon = icon
            },
            (_, args) => onClose(Convert.ToBoolean(args.Parameter)));
    }
    
    public void ShowMessage(string message, string detail, MaterialIconKind? icon = MaterialIconKind.Information)
    {
        DialogHost.Show(new MessageDialog
            {
                Message = message, 
                Detail = detail,
                Icon = icon
            });
    }

    public IDisposable ShowBusy(string message, ProgressToken progress)
    {
        progress.Cancelled += (_, _) => DialogHost.Close(null);
        DialogHost.Show(new BusyDialog { Progress = progress, Message = message });
        return new ScopedDialogCloser();
    }

    public async Task<FileInfo> ShowFileOpenAsync(string title, string filterName, string[] filterExtensions)
    {
        var mainWindow = Application.Current?.GetMainWindow();
        if (mainWindow == null)
            return null; // Cannot find the main application window.

        // ReSharper disable once PossibleNullReferenceException
        var files =
            await TopLevel
                .GetTopLevel(mainWindow)
                .StorageProvider
                .OpenFilePickerAsync(
                    new FilePickerOpenOptions
                    {
                        Title = title,
                        AllowMultiple = false,
                        FileTypeFilter = new[]
                        {
                            new FilePickerFileType(filterName)
                            {
                                Patterns = filterExtensions
                            }
                        }
                    });
        return files.FirstOrDefault()?.ToFileInfo();
    }

    public async Task<FileInfo> ShowFileSaveAsync(string title, string defaultFileName, string filterName, string[] filterExtensions)
    {
        var mainWindow = Application.Current?.GetMainWindow();
        if (mainWindow == null)
            return null; // Cannot find the main application window.
        
        // ReSharper disable once PossibleNullReferenceException
        var file =
            await TopLevel
                .GetTopLevel(mainWindow)
                .StorageProvider
                .SaveFilePickerAsync(
                    new FilePickerSaveOptions
                    {
                        Title = title,
                        ShowOverwritePrompt = true,
                        SuggestedFileName = defaultFileName,
                        DefaultExtension = filterExtensions.FirstOrDefault()?.TrimStart('*'),
                        FileTypeChoices = new[]
                        {
                            new FilePickerFileType(filterName)
                            {
                                Patterns = filterExtensions
                            }
                        }
                    });
        return file?.ToFileInfo();
    }

    public async Task<DirectoryInfo> SelectFolderAsync(string title, DirectoryInfo defaultFolder = null)
    {
        var mainWindow = Application.Current?.GetMainWindow();
        if (mainWindow == null)
            return null; // Cannot find the main application window.
        
        var options = new FolderPickerOpenOptions
        {
            Title = title
        };
        if (defaultFolder != null)
            options.SuggestedStartLocation = await mainWindow.StorageProvider.TryGetFolderFromPathAsync(defaultFolder.FullName);

        // ReSharper disable once PossibleNullReferenceException
        var folder =
            await TopLevel
                .GetTopLevel(mainWindow)
                .StorageProvider
                .OpenFolderPickerAsync(options);
        return folder.FirstOrDefault()?.ToDirectoryInfo();
    }

    private class ScopedDialogCloser : IDisposable
    {
        public void Dispose() => DialogHost.Close(null);
    }
}