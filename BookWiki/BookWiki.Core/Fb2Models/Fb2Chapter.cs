using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BookWiki.Core.Files.FileModels;
using BookWiki.Core.Files.PathModels;
using BookWiki.Core.Utils.TextModels;
using BookWiki.Presentation.Wpf.Models;

namespace BookWiki.Core.Fb2Models
{
    public class Fb2ChapterFromMarkdown : IString
    {
        public Fb2ChapterFromMarkdown(IAbsolutePath path, bool withoutComments = false)
        {
            var contentFolder = new ContentFolder(path);
            var md = contentFolder.LoadMarkdown();

            if (withoutComments)
            {
                var tmp = md.ToList().ToArray();
                var result = new List<string>();
                var commentBegan = false;
                var firstLineAfterComment = false;

                foreach (var item in tmp)
                {
                    if (commentBegan == false)
                    {
                        if (item.Trim() == "*")
                        {
                            if (string.IsNullOrWhiteSpace(result.Last()))
                            {
                                result.RemoveAt(result.Count - 1);
                            }

                            commentBegan = true;
                            continue;
                        }
                        else
                        {
                            if (firstLineAfterComment)
                            {
                                if (string.IsNullOrWhiteSpace(item))
                                {
                                    firstLineAfterComment = false;
                                    continue;
                                }
                            }

                            result.Add(item);
                        }
                    }

                    if (commentBegan)
                    {
                        if (item.Trim() == "*")
                        {
                            commentBegan = false;
                            firstLineAfterComment = true;
                            continue;
                        }
                    }
                }

                result = result.Select(x => new ClearedText(x).Value).ToList();

                md = result.ToArray();
            }

            var r = new StringBuilder();

            foreach (var p in md)
            {
                if (string.IsNullOrWhiteSpace(p))
                {
                    r.Append("<empty-line/>");
                }
                else if (p.StartsWith("# "))
                {
                    r.Append(new WrappedText(new WrappedText(p.Replace("# ", "").TrimEnd(), "p").Value, "title").Value);
                }
                else if (p.StartsWith("## "))
                {
                    r.Append(new RightStyledWrappedText(new WrappedText(p.Replace("## ", "").TrimEnd(), "em").Value, "p ").Value);
                }
                else
                {
                    r.Append(new WrappedText(p, "p").Value);
                }

            }

            Value = new WrapEachChapterInSection(r.ToString()).Value;
        }

        public string Value { get; }

    }

    public class Fb2Chapter : IString
    {
        public Fb2Chapter(IAbsolutePath path, bool withoutComments = false)
        {
            var contentFolder = new ContentFolder(path);
            var formats = contentFolder.LoadFormat().Where(x => x.Style == TextStyle.Centered || x.Style == TextStyle.Right).ToArray();
            var content = contentFolder.LoadLines();

            var result = string.Empty;

            var offset = 0;
            foreach (var p in content)
            {
                var format = GetFormat();
                var formatted = new AutostyleFb2Paragraph(p.PlainText, format, withoutComments).Value;
                result += formatted;
                offset += p.PlainText.Length + 1;
            }

            Value = new WrapEachChapterInSection(result).Value;

            ITextInfo GetFormat()
            {
                return formats.FirstOrDefault(
                    (element) =>
                        offset >= element.Range.Offset &&
                        offset <= element.Range.Offset + element.Range.Length - 1);
            }
        }

        public string Value { get; }
    }

    public class HtmlChapterWithoutTitle : IString
    {
        private readonly IAbsolutePath _path;

        public HtmlChapterWithoutTitle(IAbsolutePath path)
        {
            _path = path;
        }

        public string Value
        {
            get
            {
                var contentFolder = new ContentFolder(_path);
                var formats = contentFolder.LoadFormat().Where(x => x.Style == TextStyle.Centered || x.Style == TextStyle.Right).ToArray();
                var content = contentFolder.LoadLines();

                var result = string.Empty;

                var offset = 0;
                foreach (var p in content)
                {
                    var format = GetFormat();
                    var formatted = new AutostyleHtmlParagraph(p.PlainText, format)
                    {
                        CenteredStyle = x => new EmptyString()
                    }.Value;
                    result += formatted;
                    offset += p.PlainText.Length + 1;
                }

                return result.Replace("–", "—");

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

    public class Fb2ChapterV2 : IString
    {
        private readonly IAbsolutePath _path;

        public Fb2ChapterV2(IAbsolutePath path)
        {
            _path = path;
        }

        public string Value
        {
            get
            {
                var contentFolder = new ContentFolder(_path);
                var formats = contentFolder.LoadFormat().Where(x => x.Style == TextStyle.Centered || x.Style == TextStyle.Right).ToArray();
                var content = contentFolder.LoadLines();

                var result = string.Empty;

                var offset = 0;
                foreach (var p in content)
                {
                    var format = GetFormat();
                    var formatted = new AutostyleFb2Paragraph(p.PlainText, format)
                    {
                        CenteredStyle = x => Title(_path, x)
                    }.Value;
                    result += formatted;
                    offset += p.PlainText.Length + 1;
                }

                return new WrapEachChapterInSection(result).Value;

                ITextInfo GetFormat()
                {
                    return formats.FirstOrDefault(
                        (element) =>
                            offset >= element.Range.Offset &&
                            offset <= element.Range.Offset + element.Range.Length - 1);
                }
            }
        }

        public Func<IAbsolutePath, IString, IString> Title { get; set; } = (path, content) => new CenteredFb2Paragraph(content.Value);
    }
}