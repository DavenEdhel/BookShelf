using System.Linq;
using BookWiki.Core.Files.PathModels;
using BookWiki.Core.FileSystem.FileModels;
using BookWiki.Core.Utils.PropertyModels;
using Newtonsoft.Json;

namespace BookWiki.Core.Files.FileModels
{
    public class ContentFolder : IContentFolder
    {
        private const string ContentFileName = "Text.txt";
        private const string FormatFileName = "Format.json";

        private readonly IProperty<IText> _text;
        private readonly IFile _contentFile;
        private readonly IFile _formatFile;

        public ContentFolder(IPath path)
        {
            Source = path;

            _contentFile = new TextFile(new FilePath(path, ContentFileName));
            _formatFile = new TextFile(new FilePath(path, FormatFileName));

            _text = new CachedValue<IText>(LoadText);
        }

        public IPath Source { get; }

        public void Save(IText text)
        {
            _contentFile.Save(text.PlainText);
        }

        public void Save(ISequence<ITextInfo> novelFormat)
        {
            var format = novelFormat
                .Select(x => new TextInfoDto()
                {
                    Offset = x.Range.Offset,
                    Length = x.Range.Length,
                    Style = x.Style
                })
                .ToArray();

            var content = JsonConvert.SerializeObject(format);

            _formatFile.Save(content);
        }

        public IText LoadText()
        {
            return new StringText(_contentFile.Content);
        }

        public ISequence<ITextInfo> LoadFormat()
        {
            var content = _formatFile.Content;

            var format = JsonConvert.DeserializeObject<TextInfoDto[]>(content) ?? new TextInfoDto[0];

            var items = format.Select(x => new TextInfo(new LazySubstringText(_text, x.Offset, x.Length), x.Style));

            return new EnumerableSequence<ITextInfo>(items, format.Length);
        }

        class TextInfoDto
        {
            public int Offset { get; set; }

            public int Length { get; set; }

            public TextStyle Style { get; set; }
        }
    }
}