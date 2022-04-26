using BookWiki.Core.Files.PathModels;

namespace BookWiki.Core.FileSystem.FileModels
{
    public interface IContentFolder
    {
        IAbsolutePath Source { get; }

        void Save(IText text);

        void Save(ISequence<ITextInfo> novelFormat);

        void Save(INovel novel);

        IText LoadText();

        IText[] LoadLines();

        ISequence<ITextInfo> LoadFormat();

        IText LoadComments();
    }
}