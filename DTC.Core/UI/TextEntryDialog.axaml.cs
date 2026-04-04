// Code authored by Dean Edis (DeanTheCoder).
// Anyone is free to copy, modify, use, compile, or distribute this software,
// either in source code form or as a compiled binary, for any purpose.
//
// If you modify the code, please retain this copyright header,
// and consider contributing back to the repository or letting us know
// about your modifications. Your contributions are valued!
//
// THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND.

using Avalonia;
using Avalonia.Controls;
using Material.Icons;

namespace DTC.Core.UI;

/// <summary>
/// Simple dialog for collecting a single line of text from the user.
/// </summary>
/// <remarks>
/// This keeps small app prompts, such as API-key entry, consistent across the G33k desktop tools.
/// </remarks>
public partial class TextEntryDialog : UserControl
{
    private string m_message;
    private string m_detail;
    private string m_inputText;
    private string m_watermark;
    private string m_cancelButton;
    private string m_actionButton;
    private MaterialIconKind? m_icon;

    public static readonly DirectProperty<TextEntryDialog, string> MessageProperty = AvaloniaProperty.RegisterDirect<TextEntryDialog, string>(nameof(Message), o => o.Message, (o, v) => o.Message = v);
    public static readonly DirectProperty<TextEntryDialog, string> DetailProperty = AvaloniaProperty.RegisterDirect<TextEntryDialog, string>(nameof(Detail), o => o.Detail, (o, v) => o.Detail = v);
    public static readonly DirectProperty<TextEntryDialog, string> InputTextProperty = AvaloniaProperty.RegisterDirect<TextEntryDialog, string>(nameof(InputText), o => o.InputText, (o, v) => o.InputText = v);
    public static readonly DirectProperty<TextEntryDialog, string> WatermarkProperty = AvaloniaProperty.RegisterDirect<TextEntryDialog, string>(nameof(Watermark), o => o.Watermark, (o, v) => o.Watermark = v);
    public static readonly DirectProperty<TextEntryDialog, string> CancelButtonProperty = AvaloniaProperty.RegisterDirect<TextEntryDialog, string>(nameof(CancelButton), o => o.CancelButton, (o, v) => o.CancelButton = v);
    public static readonly DirectProperty<TextEntryDialog, string> ActionButtonProperty = AvaloniaProperty.RegisterDirect<TextEntryDialog, string>(nameof(ActionButton), o => o.ActionButton, (o, v) => o.ActionButton = v);
    public static readonly DirectProperty<TextEntryDialog, MaterialIconKind?> IconProperty = AvaloniaProperty.RegisterDirect<TextEntryDialog, MaterialIconKind?>(nameof(Icon), o => o.Icon, (o, v) => o.Icon = v);

    public TextEntryDialog()
    {
        InitializeComponent();
    }

    public string Message
    {
        get => m_message;
        set => SetAndRaise(MessageProperty, ref m_message, value);
    }

    public string Detail
    {
        get => m_detail;
        set => SetAndRaise(DetailProperty, ref m_detail, value);
    }

    public string InputText
    {
        get => m_inputText;
        set => SetAndRaise(InputTextProperty, ref m_inputText, value);
    }

    public string Watermark
    {
        get => m_watermark;
        set => SetAndRaise(WatermarkProperty, ref m_watermark, value);
    }

    public string CancelButton
    {
        get => m_cancelButton;
        set => SetAndRaise(CancelButtonProperty, ref m_cancelButton, value);
    }

    public string ActionButton
    {
        get => m_actionButton;
        set => SetAndRaise(ActionButtonProperty, ref m_actionButton, value);
    }

    public MaterialIconKind? Icon
    {
        get => m_icon;
        set => SetAndRaise(IconProperty, ref m_icon, value);
    }
}
