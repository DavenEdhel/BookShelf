using BookWiki.Core.Files.PathModels;
using BookWiki.Core.FileSystem.PathModels;

namespace BookWiki.Core
{
    public interface IContent
    {
        IText Content { get; }

        string Title { get; }

        IPath Source { get; }

        bool Equals(object another);

        int GetHashCode();
    }
}