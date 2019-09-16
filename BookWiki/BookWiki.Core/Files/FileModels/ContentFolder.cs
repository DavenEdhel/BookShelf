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

        public ContentFolder(IAbsolutePath path)
        {
            Source = path;

            _contentFile = new TextFile(new FilePath(path, ContentFileName));
            _formatFile = new TextFile(new FilePath(path, FormatFileName));

            _text = new CachedValue<IText>(LoadText);
        }

        public IAbsolutePath Source { get; }

        public void Save(IText text)
        {
            _contentFile.Save(text.PlainText);
        }

        public void Save(ISequence<ITextInfo> novelFormat)
        {
            _formatFile.Save(new TextFormat(novelFormat).ToJson());
        }

        public IText LoadText()
        {
            return new StringText(_contentFile.Content);
        }

        public ISequence<ITextInfo> LoadFormat()
        {
            var content = _formatFile.Content;

            return TextFormat.ParseFrom(content, _text.Value);
        }
    }
}