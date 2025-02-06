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

namespace CSharp.Core.Extensions;

public static class NumberExtensions
{
    public static int Clamp(this int f, int min, int max) =>
        Math.Max(min, Math.Min(max, f));
    
    public static double Clamp(this double f, double min, double max) =>
        Math.Max(min, Math.Min(max, f));

    public static double Lerp(this double f, double from, double to) =>
        from * (1.0 - f) + to * f;
    
    public static Rgb Lerp(this double f, Rgb from, Rgb to)
    {
        var r = f.Lerp(from.R, to.R);
        var g = f.Lerp(from.G, to.G);
        var b = f.Lerp(from.B, to.B);
        return new Rgb((byte)r.Clamp(0, 255), (byte)g.Clamp(0, 255), (byte)b.Clamp(0, 255));
    }

    public static char ToAscii(this double f)
    {
        const string gradient = " .,;ilS8$@";
        var index = (int)Math.Round(f.Clamp(0.0, 1.0) * (gradient.Length - 1));
        return gradient[index];
    }

    /// <summary>
    /// Convert a byte count to human-readable string.
    /// </summary>
    public static string ToSize(this long bytes)
    {
        if (bytes < 4096)
            return $"{bytes:N0} bytes";
        string[] sizes =
        [
            "bytes", "KB", "MB", "GB", "TB"
        ];

        var order = (int)Math.Log(bytes, 1024);
        var formattedValue = bytes / Math.Pow(1024, order);
        return $"{formattedValue:N2} {sizes[order]}";
    }
}