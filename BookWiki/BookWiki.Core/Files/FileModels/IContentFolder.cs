using BookWiki.Core.Files.PathModels;
using BookWiki.Core.FileSystem.PathModels;

namespace BookWiki.Core.FileSystem.FileModels
{
    public interface IContentFolder
    {
        IPath Source { get; }

        void Save(IText text);

        void Save(ISequence<ITextInfo> novelFormat);

        IText LoadText();

        ISequence<ITextInfo> LoadFormat();
    }
}