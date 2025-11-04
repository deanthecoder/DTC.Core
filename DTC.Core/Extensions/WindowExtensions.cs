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
using System;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;

namespace DTC.Core.Extensions;

public static class WindowExtensions
{
    /// <summary>
    /// Gets the position string of the window in the format "X,Y,Width,Height,ScreenIndex,State".
    /// </summary>
    public static string GetPositionString(this Window window)
    {
        var screen = window.Screens.ScreenFromWindow(window);
        var screens = window.Screens.All.ToArray();
        var screenIndex = Array.FindIndex(screens, s => s.Bounds == screen?.Bounds);
        return string.Format(CultureInfo.InvariantCulture, "{0},{1},{2},{3},{4},{5}",
                             window.Position.X, 
                             window.Position.Y, 
                             (int)window.Bounds.Width, 
                             (int)window.Bounds.Height, 
                             screenIndex,
                             (int)window.WindowState);
    }

    /// <summary>
    /// Sets the position, size, and state of the window based on the provided position string in the format "X,Y,Width,Height,ScreenIndex,State".
    /// </summary>
    public static void SetPositionFromString(this Window window, string positionString)
    {
        if (string.IsNullOrWhiteSpace(positionString))
            return;

        var parts = positionString.Split(',');
        if (parts.Length != 6)
            return; // Invalid format.
        
        if (int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var x) &&
            int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var y) &&
            int.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var width) &&
            int.TryParse(parts[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out var height) &&
            int.TryParse(parts[4], NumberStyles.Integer, CultureInfo.InvariantCulture, out var screenIndex) &&
            int.TryParse(parts[5], NumberStyles.Integer, CultureInfo.InvariantCulture, out var state))
        {
            var screens = window.Screens.All.ToArray();
            if (screenIndex < 0 || screenIndex >= screens.Length)
                return; // Monitors must have changed...
            
            var windowPosition = new PixelPoint(x, y);
            window.Position = windowPosition;
            window.Width = width;
            window.Height = height;
            
            // Restore the window state
            window.WindowState = (WindowState)state;
            window.SystemDecorations = window.WindowState == WindowState.FullScreen ? SystemDecorations.None : SystemDecorations.Full;
        }
    }
}