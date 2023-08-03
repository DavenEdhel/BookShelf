using System;
using System.Linq;
using BookWiki.Core.Files.FileModels;
using BookWiki.Core.Files.PathModels;
using BookWiki.Core.Utils.TextModels;
using BookWiki.Presentation.Wpf.Models;

namespace BookWiki.Core.Fb2Models
{
    public class Fb2Chapter : IString
    {
        public Fb2Chapter(IAbsolutePath path)
        {
            var contentFolder = new ContentFolder(path);
            var formats = contentFolder.LoadFormat().Where(x => x.Style == TextStyle.Centered || x.Style == TextStyle.Right).ToArray();
            var content = contentFolder.LoadLines();

            var result = string.Empty;

            var offset = 0;
            foreach (var p in content)
            {
                var format = GetFormat();
                var formatted = new AutostyleFb2Paragraph(p.PlainText, format).Value;
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

        public Func<IAbsolutePath, string, IString> Title { get; set; } = (path, content) => new CenteredFb2Paragraph(content);
    }
}