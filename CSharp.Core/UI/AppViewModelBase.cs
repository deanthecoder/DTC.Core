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

using System.Windows.Input;
using Avalonia;
using CSharp.Core.Commands;
using CSharp.Core.Extensions;
using CSharp.Core.ViewModels;

namespace CSharp.Core.UI;

public abstract class AppViewModelBase : ViewModelBase
{
    public AboutInfo AboutInfo { get; }
    public ICommand AboutCommand { get; }

    protected AppViewModelBase(AboutInfo aboutInfo)
    {
        AboutInfo = aboutInfo ?? throw new ArgumentNullException(nameof(aboutInfo));

        var isOpen = false;
        AboutCommand = new RelayCommand(_ =>
        {
            if (isOpen)
                return;
            var dialog = new AboutDialog(AboutInfo);
            dialog.Opened += (_, _) => isOpen = true;
            dialog.Closed += (_, _) => isOpen = false;
            var mainWindow = Application.Current?.GetMainWindow();
            if (mainWindow != null)
                dialog.ShowDialog(mainWindow);
        });
    }
}
