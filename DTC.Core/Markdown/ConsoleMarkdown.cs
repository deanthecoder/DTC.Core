// Code authored by Dean Edis (DeanTheCoder).
// Anyone is free to copy, modify, use, compile, or distribute this software,
// either in source code form or as a compiled binary, for any
// purpose.
//
// If you modify the code, please retain this copyright header,
// and consider contributing back to the repository or letting us know
// about your modifications. Your contributions are valued!
//
// THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND.

using System;
using System.IO;
using Markdig;

namespace DTC.Core.Markdown;

public class ConsoleMarkdown
{
    /// <summary>
    /// Write the specified Markdown string to the Console.
    /// </summary>
    public void Write(string md)
    {
        using var writer = new StringWriter();
        var pipeline =
            new MarkdownPipelineBuilder()
            .UsePipeTables()
            .UseEmphasisExtras()
            .Build();
        
        var renderer = new ConsoleRenderer(writer);
        Markdig.Markdown.Convert(renderer.Preprocess(md), renderer, pipeline);

        var oldEncoding = Console.OutputEncoding;
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.Write(writer.ToString());
        Console.OutputEncoding = oldEncoding;
    }
}
