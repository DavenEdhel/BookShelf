using System.Linq;
using BookWiki.Core.Files.PathModels;
using BookWiki.Core.FileSystem.FileModels;
using BookWiki.Core.Utils.PropertyModels;
using Newtonsoft.Json;

namespace BookWiki.Core.Files.FileModels
{
    public class ArticleMetadata
    {
        private readonly IAbsolutePath _path;
        private const string MetadataFileName = "Metadata.json";

        private readonly CachedValue<IText> _text;
        private readonly CachedValue<Data> _content;

        private readonly IFile _contentFile;

        public ArticleMetadata(IAbsolutePath path)
        {
            _path = path;

            _contentFile = new TextFile(new FilePath(path, MetadataFileName));

            _text = new CachedValue<IText>(LoadText);
            _content = new CachedValue<Data>(
                () =>
                {
                    return JsonConvert.DeserializeObject<Data>(_text.Value.PlainText) ?? new Data();
                }
            );
        }

        public string Name => _content.Value.Name;

        public string[] NameVariations => _content.Value.NameVariations;

        public string[] Tags => _content.Value.Tags;

        public void Save(Data data)
        {
            var text = JsonConvert.SerializeObject(data);

            _contentFile.Save(text);

            Invalidate();
        }

        private IText LoadText()
        {
            return new StringText(_contentFile.Content);
        }

        public class Data
        {
            public string Name { get; set; } = "name";

            public string[] Tags { get; set; } = new[]
            {
                "tag"
            };

            public string[] NameVariations { get; set; } = new[]
            {
                "names"
            };
        }

        public void Invalidate()
        {
            _text.Invalidate();
            _content.Invalidate();
        }
    }

    public class ContentFolder : IContentFolder
    {
        private const string ContentFileName = "Text.txt";
        private const string FormatFileName = "Format.json";
        private const string CommentsFileName = "Comments.txt";

        private readonly IProperty<IText> _text;
        private readonly IFile _contentFile;
        private readonly IFile _formatFile;
        private readonly IFile _commentsFile;

        public ContentFolder(IAbsolutePath path)
        {
            Source = path;

            _contentFile = new TextFile(new FilePath(path, ContentFileName));
            _formatFile = new TextFile(new FilePath(path, FormatFileName));
            _commentsFile = new TextFile(new FilePath(path, CommentsFileName));

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

        public void Save(INovel novel)
        {
            _contentFile.Save(novel.Content.PlainText);
            _formatFile.Save(new TextFormat(novel.Format).ToJson());
            _commentsFile.Save(novel.Comments.PlainText);
        }

        public IText LoadText()
        {
            return new StringText(_contentFile.Content);
        }

        public IText[] LoadLines()
        {
            return _contentFile.Lines.Select(x => new StringText(x)).ToArray();
        }

        public ISequence<ITextInfo> LoadFormat()
        {
            var content = _formatFile.Content;

            return TextFormat.ParseFrom(content, _text.Value);
        }

        public IText LoadComments()
        {
            return new StringText(_commentsFile.Content);
        }

        public void SaveComments(IText comments)
        {
            _commentsFile.Save(comments.PlainText);
        }
    }
}