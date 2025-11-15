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

namespace DTC.Core.Image;

public static class TgaWriter
{
    private const int MaxPacketLength = 128;

    /// <summary>
    /// Writes an 8-bit grayscale, 24-bit RGB, or 32-bit RGBA TGA file using RLE compression.
    /// </summary>
    /// <param name="file">Destination file.</param>
    /// <param name="framebuffer">Linear framebuffer.</param>
    /// <param name="width">Image width.</param>
    /// <param name="height">Image height.</param>
    /// <param name="bpp">Channels per pixel: 1 (gray), 3 (RGB), or 4 (RGBA).</param>
    public static void Write(FileInfo file, byte[] framebuffer, int width, int height, int bpp)
    {
        if (file == null)
            throw new ArgumentNullException(nameof(file));
        if (framebuffer == null)
            throw new ArgumentNullException(nameof(framebuffer));
        if (width <= 0)
            throw new ArgumentOutOfRangeException(nameof(width));
        if (height <= 0)
            throw new ArgumentOutOfRangeException(nameof(height));
        if (bpp != 1 && bpp != 3 && bpp != 4)
            throw new ArgumentOutOfRangeException(nameof(bpp), "bpp must be 1, 3 or 4.");

        var pixelCount = width * height;
        var expectedSize = pixelCount * bpp;
        if (framebuffer.Length != expectedSize)
            throw new ArgumentException($"Framebuffer size {framebuffer.Length} does not match width×height×bpp {expectedSize}.");

        var descriptor = (byte)0x20; // Origin at top-left.
        if (bpp == 4)
            descriptor |= 0x08; // 8 bits of alpha.

        var imageType = (byte)(bpp == 1 ? 11 : 10); // RLE grayscale or RLE truecolour.
        var pixelSize = bpp == 1 ? 1 : bpp;

        var header = new byte[18];
        header[2] = imageType;
        header[12] = (byte)(width & 0xFF);
        header[13] = (byte)((width >> 8) & 0xFF);
        header[14] = (byte)(height & 0xFF);
        header[15] = (byte)((height >> 8) & 0xFF);
        header[16] = (byte)(pixelSize * 8);
        header[17] = descriptor;

        var pixelData = ConvertToTgaOrdering(framebuffer, bpp);

        using var stream = file.Open(FileMode.Create, FileAccess.Write, FileShare.None);
        stream.Write(header, 0, header.Length);
        WriteRle(stream, pixelData, pixelSize);
    }

    private static byte[] ConvertToTgaOrdering(byte[] framebuffer, int bpp)
    {
        var pixelSize = bpp == 1 ? 1 : bpp;
        var pixelCount = framebuffer.Length / bpp;
        var converted = new byte[pixelCount * pixelSize];

        for (var i = 0; i < pixelCount; i++)
        {
            var src = i * bpp;
            var dst = i * pixelSize;

            switch (bpp)
            {
                case 1:
                    converted[dst] = framebuffer[src];
                    break;

                case 3:
                    converted[dst] = framebuffer[src + 2];     // B
                    converted[dst + 1] = framebuffer[src + 1]; // G
                    converted[dst + 2] = framebuffer[src];     // R
                    break;

                case 4:
                    converted[dst] = framebuffer[src + 2];     // B
                    converted[dst + 1] = framebuffer[src + 1]; // G
                    converted[dst + 2] = framebuffer[src];     // R
                    converted[dst + 3] = framebuffer[src + 3]; // A
                    break;
            }
        }

        return converted;
    }

    private static void WriteRle(FileStream stream, byte[] buffer, int pixelSize)
    {
        var offset = 0;
        while (offset < buffer.Length)
        {
            var remainingPixels = (buffer.Length - offset) / pixelSize;
            var runLength = 1;
            var maxRun = Math.Min(MaxPacketLength, remainingPixels);

            while (runLength < maxRun &&
                   PixelsEqual(buffer, offset, offset + runLength * pixelSize, pixelSize))
            {
                runLength++;
            }

            if (runLength > 1)
            {
                stream.WriteByte((byte)(0x80 | (runLength - 1)));
                stream.Write(buffer, offset, pixelSize);
                offset += runLength * pixelSize;
                continue;
            }

            var rawLength = 1;
            while (rawLength < maxRun)
            {
                var current = offset + (rawLength - 1) * pixelSize;
                var next = current + pixelSize;
                if (next >= buffer.Length || PixelsEqual(buffer, current, next, pixelSize))
                    break;
                rawLength++;
            }

            stream.WriteByte((byte)(rawLength - 1));
            var bytesToWrite = rawLength * pixelSize;
            stream.Write(buffer, offset, bytesToWrite);
            offset += bytesToWrite;
        }
    }

    private static bool PixelsEqual(byte[] buffer, int indexA, int indexB, int pixelSize)
    {
        for (var i = 0; i < pixelSize; i++)
        {
            if (buffer[indexA + i] != buffer[indexB + i])
                return false;
        }

        return true;
    }
}
