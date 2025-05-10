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
using Avalonia.Media;

namespace DTC.Core;

public record Rgb(byte R, byte G, byte B)
{
    public static Rgb Black { get; } = Colors.Black;
    public static Rgb White { get; } = Colors.White;
    
    public static implicit operator Rgb(Color color) => new Rgb(color.R, color.G, color.B);
    public static implicit operator Color(Rgb rgb) => Color.FromRgb(rgb.R, rgb.G, rgb.B);

    public double Luminosity() => R * 0.2126 + G * 0.7152 + B * 0.0722;
}