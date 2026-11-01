using BookWiki.Core.Fb2Models;
using BookWiki.Core.Files.FileModels;
using BookWiki.Core.Files.PathModels;
using BookWiki.Core.FileSystem.FileModels;
using BookWiki.Core.Utils.TextModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BookWiki.Core.StructuredNovelModels
{
    internal class StructuredText
    {

    }

    public class DocumentFlowFromMarkdown : IDocumentFlowContent
    {
        public DocumentFlowFromMarkdown(string[] lines)
        {
            var p = new List<IParagraph>();

            foreach (var line in lines)
            {
                p.Add(new ParagraphFromMarkdown(line));
            }

            Paragraphs = p.ToArray();
        }

        public IParagraph[] Paragraphs { get; }

        public class ParagraphFromMarkdown : IParagraph
        {
            public ParagraphFromMarkdown(string line)
            {
                if (line.StartsWith("# "))
                {
                    FormattingStyle = TextStyle.Centered;
                    Inlines = new List<IInline>()
                    {
                        new PlainInline(line.Replace("# ", "").TrimEnd())
                    }.ToArray();
                }
                else if (line.StartsWith("## "))
                {
                    FormattingStyle = TextStyle.Right;
                    Inlines = new List<IInline>()
                    {
                        new PlainInline(line.Replace("## ", "").TrimEnd())
                    }.ToArray();
                }
                else
                {
                    FormattingStyle = TextStyle.None;
                    Inlines = new List<IInline>()
                    {
                        new PlainInline(line.TrimEnd())
                    }.ToArray();
                }
            }

            public TextStyle FormattingStyle { get; }

            public IInline[] Inlines { get; }
        }
    }

    public class MarkdownFromDocumentFlow
    {
        private readonly IDocumentFlowContent content;

        public MarkdownFromDocumentFlow(IDocumentFlowContent content)
        {
            this.content = content;
        }

        public string PlainText
        {
            get
            {
                var result = new StringBuilder();

                foreach (var p in content.Paragraphs)
                {
                    switch (p.FormattingStyle)
                    {
                        case TextStyle.Centered:
                            result.AppendLine("# " + p.GetAllText() + "  ");
                            break;
                        case TextStyle.Right:
                            result.AppendLine("## " + p.GetAllText() + "  ");
                            break;
                        default:
                            result.AppendLine(p.GetAllText() + "  ");
                            break;
                    }
                }

                return result.ToString();
            }
        }
    }

    public class StructuredTextFromFormattedNovel : IStructuredText
    {
        private readonly INovel novel;
        private readonly IRootPath root;

        public StructuredTextFromFormattedNovel(INovel novel, IRootPath root)
        {
            this.novel = novel;
            this.root = root;
        }

        public IEnumerable<Part> Parts
        {
            get
            {
                var contentFolder = new ContentFolder(novel.Source.AbsolutePath(root));
                var formats = contentFolder.LoadFormat().Where(x => x.Style == TextStyle.Centered || x.Style == TextStyle.Right).ToArray();
                var content = contentFolder.LoadLines();

                var result = new List<Part>();

                var offset = 0;
                foreach (var p in content)
                {
                    var format = GetFormat();
                    result.Add(new Part()
                    {
                        Content = p.PlainText,
                        Style = format.Style
                    });

                    offset += p.PlainText.Length + 1;
                }

                return result;

                ITextInfo GetFormat()
                {
                    return formats.FirstOrDefault(
                        (element) =>
                            offset >= element.Range.Offset &&
                            offset <= element.Range.Offset + element.Range.Length - 1);
                }
            }
        }
    }

    // todo: use IDocumentFlowContent
    public interface IStructuredText
    {
        IEnumerable<Part> Parts { get; }
    }

    public interface IPart
    {
        TextStyle Style { get; }

        string Content { get; }
    }

    public class Part : IPart
    {
        public TextStyle Style { get; set; }

        public string Content { get; set; }
    }
}
