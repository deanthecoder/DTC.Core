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

namespace CSharp.Core;

public class ProgressToken
{
    private double m_progress;

    public event EventHandler ProgressUpdated;

    public bool IsCancelSupported { get; set; }

    public bool CancelRequested { get; private set; }

    public double Progress
    {
        get => m_progress;
        set
        {
            if (Math.Abs(m_progress - value) < 0.01)
                return;
            m_progress = value.Clamp(0.0, 1.0);
            ProgressUpdated?.Invoke(this, EventArgs.Empty);
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
    }
}