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
using CSharp.Core.Extensions;
using CSharp.Core.ViewModels;

namespace CSharp.Core;

public class ProgressToken : ViewModelBase
{
    private int m_progress;
    private bool m_isIndeterminate = true;
    private bool m_isCancelSupported;

    public event EventHandler Cancelled;

    public bool IsCancelSupported
    {
        get => m_isCancelSupported;
        set => SetField(ref m_isCancelSupported, value);
    }

    public bool IsIndeterminate
    {
        get => m_isIndeterminate;
        set => SetField(ref m_isIndeterminate, value);
    }

    public bool CancelRequested { get; private set; }

    public int Progress
    {
        get => m_progress;
        set
        {
            if (SetField(ref m_progress, value.Clamp(0, 100)))
                IsIndeterminate = false;
        }
    }

    public ProgressToken(bool isCancelSupported = false)
    {
        IsCancelSupported = isCancelSupported;
    }

    public void Cancel()
    {
        if (!IsCancelSupported)
            throw new InvalidOperationException("Cancel is not supported");
        CancelRequested = true;
        Cancelled?.Invoke(this, EventArgs.Empty);
    }
}