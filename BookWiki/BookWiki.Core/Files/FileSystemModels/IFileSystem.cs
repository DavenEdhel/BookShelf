using BookWiki.Core.Files.PathModels;

namespace BookWiki.Core.Files.FileSystemModels
{
    public interface IFileSystem
    {
        ISequence<IAbsolutePath> Contents { get; }
    }
}