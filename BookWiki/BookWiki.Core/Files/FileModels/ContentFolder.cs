using BookWiki.Core.Files.PathModels;
using BookWiki.Core.FileSystem.FileModels;
using BookWiki.Core.Utils.PropertyModels;

namespace BookWiki.Core.Files.FileModels
{
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