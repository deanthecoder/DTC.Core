// Code authored by Dean Edis (DeanTheCoder).
// Anyone is free to copy, modify, use, compile, or distribute this software,
// either in source code form or as a compiled binary, for any purpose.
// 
// If you modify the code, please retain this copyright header,
// and consider contributing back to the repository or letting us know
// about your modifications. Your contributions are valued!
// 
// THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND.
using System;
using System.IO;
using System.Text;

namespace DTC.Core.Image;

public static class PpmWriter
{
    /// <summary>
    /// Writes an 8-bit grayscale (P5) or 24-bit RGB (P6) PPM image.
    /// </summary>
    /// <param name="file">The file to write to</param>
    /// <param name="framebuffer">Buffer containing pixel data</param>
    /// <param name="width">Width of image in pixels</param>
    /// <param name="height">Height of image in pixels</param>
    /// <param name="bpp">Bits per pixel - must be 1 (grayscale) or 3 (RGB)</param> 
    /// <remarks>
    /// If bpp == 1, pixels are expanded to RGB by repeating each value.
    /// </remarks>
    public static void Write(FileInfo file, byte[] framebuffer, int width, int height, int bpp)
    {
        if (file == null)
            throw new ArgumentNullException(nameof(file));
        if (framebuffer == null)
            throw new ArgumentNullException(nameof(framebuffer));
        if (bpp != 1 && bpp != 3)
            throw new ArgumentOutOfRangeException(nameof(bpp), "bpp must be 1 or 3.");

        var expected = width * height * bpp;
        if (framebuffer.Length != expected)
            throw new ArgumentException($"Framebuffer size {framebuffer.Length} does not match width×height×bpp {expected}.");

        var isColor = bpp == 3;
        var header = $"P6\n{width} {height}\n255\n";
        var headerBytes = Encoding.ASCII.GetBytes(header);

        using var fs = file.Open(FileMode.Create, FileAccess.Write, FileShare.None);
        fs.Write(headerBytes, 0, headerBytes.Length);

        if (isColor)
        {
            // Write RGB data directly.
            fs.Write(framebuffer, 0, framebuffer.Length);
        }
        else
        {
            // Expand grayscale to RGB triplets.
            foreach (var grey in framebuffer)
            {
                fs.WriteByte(grey);
                fs.WriteByte(grey);
                fs.WriteByte(grey);
            }
        }
    }
}
