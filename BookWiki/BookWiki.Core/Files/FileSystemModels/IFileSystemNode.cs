using BookWiki.Core.Files.PathModels;
using BookWiki.Core.FileSystem.PathModels;

namespace BookWiki.Core.Files.FileSystemModels
{
    public interface IFileSystemNode
    {
        ISequence<IFileSystemNode> InnerNodes { get; }

        IPath Path { get; }

        bool IsContentFolder { get; }

        int Level { get; }

        void SaveUnder(IFileSystemNode parent);
    }
}