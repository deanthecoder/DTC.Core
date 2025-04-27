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
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace CSharp.Core.Extensions;

public static class BrushExtensions
{
    public static Color GetColor(this IBrush brush)
    {
        var color = (brush as SolidColorBrush)?.Color ?? (brush as ImmutableSolidColorBrush)?.Color;
        if (color == null)
            throw new ArgumentException("Cannot find brush's color.");
        return (Color)color;
    }

    public static IBrush WithBrightness(this IBrush brush, double f) =>
        new SolidColorBrush(brush.GetColor().WithBrightness(f));

    public static Color WithBrightness(this Color rgb, double f)
    {
        var r = (byte)(rgb.R * f).Clamp(0.0, 255.0);
        var g = (byte)(rgb.G * f).Clamp(0.0, 255.0);
        var b = (byte)(rgb.B * f).Clamp(0.0, 255.0);
        return Color.Parse($"#{r:X2}{g:X2}{b:X2}");
    }

    public static Rgb WithBrightness(this Rgb rgb, double f)
    {
        var r = (byte)(rgb.R * f).Clamp(0.0, 255.0);
        var g = (byte)(rgb.G * f).Clamp(0.0, 255.0);
        var b = (byte)(rgb.B * f).Clamp(0.0, 255.0);
        return new Rgb(r, g, b);
    }
}