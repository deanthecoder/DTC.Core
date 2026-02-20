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
    /// Writes a binary 24-bit RGB (P6) PPM image.
    /// </summary>
    /// <param name="file">The file to write to</param>
    /// <param name="framebuffer">Buffer containing pixel data</param>
    /// <param name="width">Width of image in pixels</param>
    /// <param name="height">Height of image in pixels</param>
    /// <param name="bytesPerPixel">Bytes per pixel - must be 1 (grayscale / 8-bit) or 3 (RGB / 24-bit)</param>
    /// <remarks>
    /// If bytesPerPixel == 1, grayscale source pixels are expanded to RGB triplets.
    /// </remarks>
    public static void Write(FileInfo file, byte[] framebuffer, int width, int height, int bytesPerPixel)
    {
        if (file == null)
            throw new ArgumentNullException(nameof(file));
        if (framebuffer == null)
            throw new ArgumentNullException(nameof(framebuffer));
        if (bytesPerPixel != 1 && bytesPerPixel != 3)
            throw new ArgumentOutOfRangeException(nameof(bytesPerPixel), "bytesPerPixel must be 1 or 3.");

        var expected = width * height * bytesPerPixel;
        if (framebuffer.Length != expected)
            throw new ArgumentException($"Framebuffer size {framebuffer.Length} does not match width×height×bytesPerPixel {expected}.");

        var isColor = bytesPerPixel == 3;
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
