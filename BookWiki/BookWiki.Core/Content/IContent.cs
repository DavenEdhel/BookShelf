using BookWiki.Core.Files.PathModels;

namespace BookWiki.Core
{
    public interface IContent
    {
        IText Content { get; }

        string Title { get; }

        IRelativePath Source { get; }

        bool Equals(object another);

        int GetHashCode();
    }
}