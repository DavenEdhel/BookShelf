using BookWiki.Core.Files.PathModels;

namespace BookWiki.Core.FileSystem.FileModels
{
    public interface IContentFolder
    {
        IAbsolutePath Source { get; }

        void Save(IText text);

        void Save(ISequence<ITextInfo> novelFormat);

        IText LoadText();

        ISequence<ITextInfo> LoadFormat();
    }
}