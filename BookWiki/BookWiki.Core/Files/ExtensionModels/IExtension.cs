using BookWiki.Core.Files.FileSystemModels;

namespace BookWiki.Core.Files.PathModels
{
    public interface IExtension
    {
        string PlainText { get; }

        NodeType Type { get; }
    }
}