// Code authored by Dean Edis (DeanTheCoder).
// Anyone is free to copy, modify, use, compile, or distribute this software,
// either in source code form or as a compiled binary, for any
//  purpose.
// 
// If you modify the code, please retain this copyright header,
// and consider contributing back to the repository or letting us know
// about your modifications. Your contributions are valued!
// 
// THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND.
using Avalonia;
using Avalonia.Controls;

namespace DTC.Core.UI;

public partial class BusyDialog : UserControl
{
    private ProgressToken m_progress;
    private string m_message;
    
    public static readonly DirectProperty<BusyDialog, string> MessageProperty = AvaloniaProperty.RegisterDirect<BusyDialog, string>(nameof(Message), o => o.Message, (o, v) => o.Message = v);
    public static readonly DirectProperty<BusyDialog, ProgressToken> ProgressProperty = AvaloniaProperty.RegisterDirect<BusyDialog, ProgressToken>(nameof(Progress), o => o.Progress, (o, v) => o.Progress = v);

    public BusyDialog()
    {
        InitializeComponent();
    }

    public ProgressToken Progress
    {
        get => m_progress;
        set => SetAndRaise(ProgressProperty, ref m_progress, value);
    }
    
    public string Message
    {
        get => m_message;
        set => SetAndRaise(MessageProperty, ref m_message, value);
    }
}