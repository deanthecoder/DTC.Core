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
using System.Diagnostics;
using System.Threading.Tasks;

namespace DTC.Core;

/// <summary>
/// Animate a value from 0.0-1.0.
/// </summary>
public class Animation
{
    private readonly Func<double, bool> m_callback;
    private readonly TimeSpan m_duration;
    private readonly TimeSpan m_delay;

    public Animation(TimeSpan duration, Func<double, bool> callback) : this(TimeSpan.Zero, duration, callback)
    {
    }
    
    public Animation(TimeSpan delay, TimeSpan duration, Func<double, bool> callback)
    {
        m_duration = duration;
        m_delay = delay;
        m_callback = callback ?? throw new ArgumentNullException(nameof(callback));
    }

    public async Task StartAsync()
    {
        // Wait for the delay before starting the animation
        await Task.Delay(m_delay);

        var stopwatch = Stopwatch.StartNew();
        while (true)
        {
            var progress = stopwatch.ElapsedMilliseconds / m_duration.TotalMilliseconds;

            if (progress >= 1.0)
            {
                progress = 1.0; // Clamp progress at 1

                // Call the callback with final value
                m_callback(progress);

                // Animation finished
                return;
            }

            // Call the callback with current value
            if (!m_callback(progress))
                break;

            // Wait a little before next update
            await Task.Delay(10);
        }
    }
}