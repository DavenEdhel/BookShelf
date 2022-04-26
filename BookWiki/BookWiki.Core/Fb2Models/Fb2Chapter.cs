using System.Linq;
using BookWiki.Core.Files.FileModels;
using BookWiki.Core.Files.PathModels;
using BookWiki.Core.Utils.TextModels;

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
}