using BookWiki.Core.Files.PathModels;

namespace BookWiki.Core
{
    public interface ILibrary
    {
        IContent[] Items { get; }

        void Update(IContent content);

        void Save();

        IContent Load(IPath novelPath);
    }
}