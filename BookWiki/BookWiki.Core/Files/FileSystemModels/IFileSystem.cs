using BookWiki.Core.Files.PathModels;
using BookWiki.Core.FileSystem.PathModels;

namespace BookWiki.Core.Files.FileSystemModels
{
    public interface IFileSystem
    {
        ISequence<IPath> Contents { get; }
    }
}