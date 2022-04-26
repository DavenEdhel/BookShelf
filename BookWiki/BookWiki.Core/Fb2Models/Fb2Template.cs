using System.Collections.Generic;
using System.IO;
using System.Linq;
using BookWiki.Core.Files.FileModels;
using BookWiki.Core.Files.PathModels;
using BookWiki.Core.Utils.TextModels;

namespace BookWiki.Core.Fb2Models
{
    public class Fb2Template
    {
        public IBase64 Cover { get; set; }

        public string Annotation { get; set; }

        public string Title { get; set; }

        public List<IAbsolutePath> Chapters { get; set; } = new List<IAbsolutePath>();

        public void CompileToFile(IAbsolutePath path)
        {
            var content = new Fb2TemplateString()
            {
                Title = Title,
                Annotation = Annotation,
                Cover = Cover ?? new AutodetectedCover(path),
                Body = new Fb2BookContent(Chapters).Value
            }.Value;

            new Fb2File(path, "test", content).Save();
        }
    }

    public class Fb2File
    {
        private readonly string _content;
        private IAbsolutePath _pathToSave;

        public Fb2File(IAbsolutePath path, string fileName, string content)
        {
            _content = content;
            _pathToSave = new FilePath(path, $"{fileName}.fb2");
        }

        public void Save()
        {
            File.WriteAllText(_pathToSave.FullPath, _content);
        }
    }

    public class Fb2BookContent : IString
    {
        private readonly List<IAbsolutePath> _chapters;

        public Fb2BookContent(List<IAbsolutePath> chapters)
        {
            _chapters = chapters;
        }

        public string Value => string.Join("\n", _chapters.Select(x => new Fb2Chapter(x).Value));
    }

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
                var formatted = new Fb2Paragraph(p.PlainText, format).Value;
                result += formatted;
                offset += p.PlainText.Length + 1;
            }

            Value = new WrappedText(result, "section").Value;

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

    public class Fb2Paragraph : IString
    {
        public Fb2Paragraph(string content, ITextInfo format)
        {
            if (format != null)
            {
                switch (format.Style)
                {
                    case TextStyle.Centered:
                        Value = new WrappedText(new WrappedText(content, "p").Value, "title").Value;
                        break;

                    case TextStyle.Right:
                        Value = new WrappedText(new WrappedText(content, "emphasis").Value, "p").Value;
                        break;

                    default:
                        Value = content;
                        break;
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(content))
                {
                    Value = "<br />";
                }
                else
                {
                    Value = new WrappedText(content, "p").Value;
                }
            }
        }

        public string Value { get; }
    }

    public class WrappedText : IString
    {
        public WrappedText(string text, string tag)
        {
            Tag = tag;
            Text = text;
        }

        public string Tag { get; } = "empty";

        public string Text { get; } = string.Empty;

        public string Value => $"<{Tag}>{Text}</{Tag}>";
    }
}