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
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Markdig.Extensions.Tables;
using Markdig.Renderers;
using Markdig.Renderers.Normalize;
using Markdig.Renderers.Normalize.Inlines;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace DTC.Core.Markdown;

/// <summary>
/// Markdig-compatible renderer, targetting output to the Console.
/// </summary>
public class ConsoleRenderer : NormalizeRenderer
{
    [NotNull] private readonly StringWriter m_writer;
    private readonly ConsoleFormatter m_formatter;

    public ConsoleRenderer([NotNull] StringWriter writer) : base(writer)
    {
        m_writer = writer;
        m_formatter = new ConsoleFormatter(this);
        ReplaceRenderer(typeof(HeadingBlock), () => new ConsoleHeadingRenderer(m_formatter));
        ReplaceRenderer(typeof(CodeInline), () => new ConsoleCodeInlineRenderer(m_formatter));
        ReplaceRenderer(typeof(CodeBlock), () => new ConsoleCodeBlockRenderer(m_formatter));
        ReplaceRenderer(typeof(EmphasisInline), () => new ConsoleEmphasisInlineRenderer(m_formatter));
        ReplaceRenderer(typeof(LinkInline), () => new ConsoleLinkInlineRenderer(m_formatter));
        ReplaceRenderer(typeof(ThematicBreakBlock), () => new ConsoleThematicBreakRenderer(m_formatter));
        ReplaceRenderer(typeof(LinkReferenceDefinitionGroup), () => new ConsoleLinkReferenceDefinitionGroupRenderer());
        ReplaceRenderer(typeof(LiteralInline), () => new ConsoleLiteralInlineRenderer());
        ReplaceRenderer(typeof(Table), () => new ConsoleTableRenderer(m_formatter));
        ReplaceRenderer(typeof(ListBlock), () => new ConsoleListRenderer(m_formatter));
    }

    public string Preprocess(string md) =>
        m_formatter.Support == ConsoleFormatter.StyleSupport.None ? md : md.Replace("[x]", "☑").Replace("[ ]", "☐");

    private void ReplaceRenderer(Type markdownObjectType, Func<IMarkdownObjectRenderer> createRenderer)
    {
        var existingRenderer = ObjectRenderers.FirstOrDefault(o => o.Accept(this, markdownObjectType));
        if (existingRenderer != null)
            ObjectRenderers.Remove(existingRenderer);
        ObjectRenderers.Add(createRenderer());
    }

    private class ConsoleHeadingRenderer : HeadingRenderer
    {
        private readonly ConsoleFormatter m_formatter;

        public ConsoleHeadingRenderer([NotNull] ConsoleFormatter formatter)
        {
            m_formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
        }

        public override void Write(RendererBase renderer, MarkdownObject obj)
        {
            var consoleRenderer = (ConsoleRenderer) renderer;
            consoleRenderer.EnsureEmptyLine();
            
            var heading = (HeadingBlock) obj;
            var applyBold = heading.Level == 1;
            var applyUnderline = heading.Level == 1 || heading.Level == 2;

            var styles = ConsoleFormatter.Style.Highlight;
            if (applyBold)
                styles |= ConsoleFormatter.Style.Bold;
            if (applyUnderline)
                styles |= ConsoleFormatter.Style.Underline;

            m_formatter.ApplyStyles(styles);
            consoleRenderer.WriteLeafInline(heading);
            m_formatter.ResetStyles(styles);

            consoleRenderer.WriteLine();
        }
    }

    /// <summary>
    /// Ensure a line gap is present before the current position.
    /// </summary>
    /// <remarks>
    /// This is a no-op if there's already an empty line.
    /// </remarks>
    private void EnsureEmptyLine()
    {
        var source = m_writer.GetStringBuilder();
        if (source.Length == 0)
            return; // Nothing to do.

        var newLines = 0;
        for (var i = 0; i < source.Length; i++)
        {
            var ch = source[source.Length - 1 - i];
            if (ch == '\r' || ch == ' ')
                continue; // Ignore.
            if (ch == '\n')
            {
                newLines++;
                continue;
            }

            break;
        }
        
        if (newLines < 2)
            m_writer.WriteLine();
    }

    private class ConsoleCodeInlineRenderer : CodeInlineRenderer
    {
        private readonly ConsoleFormatter m_formatter;

        public ConsoleCodeInlineRenderer([NotNull] ConsoleFormatter formatter)
        {
            m_formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
        }

        public override void Write(RendererBase renderer, MarkdownObject obj)
        {
            var consoleRenderer = (ConsoleRenderer) renderer;
            m_formatter.ApplyStyles(ConsoleFormatter.Style.Bold | ConsoleFormatter.Style.DimBackground);
            consoleRenderer.Write(((CodeInline) obj).Content);
            m_formatter.ResetStyles(ConsoleFormatter.Style.Bold | ConsoleFormatter.Style.DimBackground);
        }
    }

    private class ConsoleCodeBlockRenderer : CodeBlockRenderer
    {
        private const int SheetWidth = 80;
        private readonly ConsoleFormatter m_formatter;

        public ConsoleCodeBlockRenderer([NotNull] ConsoleFormatter formatter)
        {
            m_formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
        }

        public override void Write(RendererBase renderer, MarkdownObject obj)
        {
            var consoleRenderer = (ConsoleRenderer) renderer;
            consoleRenderer.EnsureEmptyLine();

            foreach (var line in ((CodeBlock) obj).Lines)
            {
                m_formatter.ApplyStyles(ConsoleFormatter.Style.Bold | ConsoleFormatter.Style.DimBackground);
                if (m_formatter.Support == ConsoleFormatter.StyleSupport.None)
                    consoleRenderer.Write("│");
                consoleRenderer.Write($" {line}".PadRight(SheetWidth));
                m_formatter.ResetStyles(ConsoleFormatter.Style.Bold | ConsoleFormatter.Style.DimBackground);
                consoleRenderer.WriteLine();
            }
        }
    }

    private class ConsoleEmphasisInlineRenderer : EmphasisInlineRenderer
    {
        private readonly ConsoleFormatter m_formatter;

        public ConsoleEmphasisInlineRenderer(ConsoleFormatter formatter)
        {
            m_formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
        }

        public override void Write(RendererBase renderer, MarkdownObject obj)
        {
            var emphasisInline = (EmphasisInline) obj;

            if (emphasisInline.DelimiterChar == '~')
            {
                m_formatter.ApplyStyles(ConsoleFormatter.Style.Strike);
                renderer.WriteChildren(emphasisInline);
                m_formatter.ResetStyles(ConsoleFormatter.Style.Strike);
                return;
            }
            
            var styles = emphasisInline.DelimiterCount switch
            {
                1 => ConsoleFormatter.Style.Italic,
                2 => ConsoleFormatter.Style.Bold,
                _ => ConsoleFormatter.Style.Bold | ConsoleFormatter.Style.Italic
            };

            m_formatter.ApplyStyles(styles);
            renderer.WriteChildren(emphasisInline);
            m_formatter.ResetStyles(styles);
        }
    }

    private class ConsoleLinkInlineRenderer : LinkInlineRenderer
    {
        private readonly ConsoleFormatter m_formatter;

        public ConsoleLinkInlineRenderer(ConsoleFormatter formatter)
        {
            m_formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
        }

        public override void Write(RendererBase renderer, MarkdownObject obj)
        {
            var consoleRenderer = (ConsoleRenderer) renderer;
            var linkInline = (LinkInline) obj;

            // Strip any title - We don't display them.
            // E.g. '[inline link with title](https://example.com "Title")'
            linkInline.Title = null;

            m_formatter.ApplyStyles(ConsoleFormatter.Style.Italic);
            consoleRenderer.WriteChildren(linkInline);
            m_formatter.ResetStyles(ConsoleFormatter.Style.Italic);
            
            consoleRenderer.Write(" (").Write(linkInline.Url).Write(")");
        }
    }

    private class ConsoleThematicBreakRenderer : ThematicBreakRenderer
    {
        private const int SheetWidth = 80;
        private readonly ConsoleFormatter m_formatter;

        public ConsoleThematicBreakRenderer(ConsoleFormatter formatter)
        {
            m_formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
        }

        public override void Write(RendererBase renderer, MarkdownObject obj)
        {
            var consoleRenderer = (ConsoleRenderer) renderer;

            m_formatter.ApplyStyles(ConsoleFormatter.Style.Underline | ConsoleFormatter.Style.Dim);
            consoleRenderer.WriteLine(new string(' ', SheetWidth));
            m_formatter.ResetStyles(ConsoleFormatter.Style.Underline | ConsoleFormatter.Style.Dim);
        }
    }

    private class ConsoleLinkReferenceDefinitionGroupRenderer : LinkReferenceDefinitionGroupRenderer
    {
        public override void Write(RendererBase renderer, MarkdownObject obj)
        {
            // Do nothing - Don't render them.
        }
    }

    private class ConsoleLiteralInlineRenderer : LiteralInlineRenderer
    {
        public override void Write(RendererBase renderer, MarkdownObject obj)
        {
            var consoleRenderer = (ConsoleRenderer) renderer;
            var literal = (LiteralInline) obj;

            consoleRenderer.Write(literal.ToString());
        }
    }

    private class ConsoleTableRenderer : NormalizeObjectRenderer<Table>
    {
        private readonly ConsoleFormatter m_formatter;

        public ConsoleTableRenderer(ConsoleFormatter formatter)
        {
            m_formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
        }

        protected override void Write(NormalizeRenderer renderer, Table obj)
        {
            // Ignored.
        }

        public override void Write(RendererBase renderer, MarkdownObject obj)
        {
            var consoleRenderer = (ConsoleRenderer) renderer;
            var table = (Table) obj;
            
            consoleRenderer.EnsureEmptyLine();
            
            using var stringWriter = new StringWriter();
            var subRenderer = new ConsoleRenderer(stringWriter);
            var grid = table.OfType<TableRow>().Select(row => row.OfType<TableCell>().Select(cell =>
            {
                stringWriter.GetStringBuilder().Clear();
                subRenderer.Write(cell);
                var markdown = stringWriter.ToString().Trim();
                var plainText = ConsoleFormatter.StripControls(markdown);
                return (markdown, plainText);
            }).ToArray()).ToArray();

            var rowCount = grid.Length;
            var columnCount = grid.Max(row => row.Length);
            var columnWidths =
                Enumerable.Range(0, columnCount)
                    .Select(columnIndex => grid.Max(row => columnIndex < row.Length ? row[columnIndex].plainText.Length : 0))
                    .ToArray();

            for (var rowIndex = 0; rowIndex < rowCount; rowIndex++)
            {
                if (rowIndex == 1)
                {
                    // Draw title underlines.
                    m_formatter.ApplyStyles(ConsoleFormatter.Style.Dim);
                    for (var columnIndex = 0; columnIndex < columnCount; columnIndex++)
                    {
                        if (columnIndex > 0)
                            consoleRenderer.Write('┼');
                        consoleRenderer.Write(new string('─', columnWidths[columnIndex] + 2));
                    }

                    consoleRenderer.Write('─');
                    m_formatter.ResetStyles(ConsoleFormatter.Style.Dim);
                    consoleRenderer.WriteLine();
                }
                
                var row = grid[rowIndex];
                for (var columnIndex = 0; columnIndex < columnCount; columnIndex++)
                {
                    var columnAlignment = table.ColumnDefinitions[columnIndex].Alignment;
                    var columnWidth = columnWidths[columnIndex];
                    
                    m_formatter.ApplyStyles(ConsoleFormatter.Style.Dim);
                    consoleRenderer.Write(columnIndex == 0 ? " " : " │ ");
                    m_formatter.ResetStyles(ConsoleFormatter.Style.Dim);

                    var cell = row[columnIndex];
                    var leftPad = string.Empty;
                    var rightPad = string.Empty;
                    if (columnAlignment == TableColumnAlign.Left)
                    {
                        rightPad = new string(' ', columnWidth - cell.plainText.Length);
                    }
                    else if (columnAlignment == TableColumnAlign.Right)
                    {
                        leftPad = new string(' ', columnWidth - cell.plainText.Length);
                    }
                    else
                    {
                        var pad = columnWidth - cell.plainText.Length;
                        leftPad = new string(' ', pad / 2);
                        rightPad = new string(' ', pad / 2 + pad % 2);
                    }

                    if (rowIndex == 0)
                        m_formatter.ApplyStyles(ConsoleFormatter.Style.Bold);
                    consoleRenderer.Write(leftPad);
                    consoleRenderer.Write(cell.markdown);
                    consoleRenderer.Write(rightPad);
                    if (rowIndex == 0)
                        m_formatter.ResetStyles(ConsoleFormatter.Style.Bold);
                }

                consoleRenderer.WriteLine();
            }
        }
    }

    private class ConsoleListRenderer : ListRenderer
    {
        private readonly ConsoleFormatter m_formatter;

        public ConsoleListRenderer(ConsoleFormatter formatter)
        {
            m_formatter = formatter;
        }

        public override void Write(RendererBase renderer, MarkdownObject obj)
        {
            var consoleRenderer = (ConsoleRenderer) renderer;
            var listBlock = (ListBlock) obj;

            consoleRenderer.PushIndent("  ");
            if (!listBlock.IsOrdered)
                listBlock.BulletType = m_formatter.Support == ConsoleFormatter.StyleSupport.None ? '*' : '•';
            base.Write(renderer, obj);
            consoleRenderer.PopIndent();
        }
    }
}