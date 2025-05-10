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
using System.Threading;

namespace DTC.Core;

/// <summary>
/// The ActionConsolidator class manages the execution of an Action delegate using a debounce strategy.
/// It ensures that the action is invoked only after a specified period of inactivity.
/// Useful for scenarios like input throttling, where actions should only be taken once user input has settled.
/// </summary>
public sealed class ActionConsolidator : IDisposable
{
    private readonly Action m_action;
    private readonly Timer m_timer;
    private readonly object m_lock = new object();
    private readonly TimeSpan m_debounceTime;

    public ActionConsolidator(Action action, double debounceSecs = 0.1)
    {
        m_action = action ?? throw new ArgumentNullException(nameof(action));
        m_debounceTime = TimeSpan.FromSeconds(debounceSecs);
        m_timer = new Timer(_ => InvokeNow(), null, Timeout.Infinite, Timeout.Infinite);
    }

    public void Invoke()
    {
        lock (m_lock)
        {
            // Always reset the timer on each call
            m_timer.Change(m_debounceTime, Timeout.InfiniteTimeSpan);
        }
    }

    private void InvokeNow()
    {
        lock (m_lock)
        {
            m_action.Invoke();
            m_timer.Change(Timeout.Infinite, Timeout.Infinite); // Stop the timer after firing
        }
    }

    public void Dispose() => m_timer?.Dispose();
}