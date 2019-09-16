using BookWiki.Core.Files.PathModels;

namespace BookWiki.Core.Files.FileSystemModels
{
    public interface IFileSystemNode
    {
        ISequence<IFileSystemNode> InnerNodes { get; }

        IAbsolutePath Path { get; }

        bool IsContentFolder { get; }

        int Level { get; }

        void SaveUnder(IFileSystemNode parent);
    }
}