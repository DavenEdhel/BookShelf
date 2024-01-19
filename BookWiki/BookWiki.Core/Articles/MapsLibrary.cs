using BookWiki.Core.Files.FileSystemModels;
using BookWiki.Core.Files.PathModels;

namespace BookWiki.Core.Articles
{
    public class MapsLibrary
    {
        private readonly IRootPath _rootPath;

        public MapsLibrary(IRootPath rootPath)
        {
            _rootPath = rootPath;

            MapsRoot.EnsureCreated();
        }

        public FolderPath MapsRoot => new FolderPath(
            _rootPath,
            new FileName("Карты"),
            new Extension(NodeType.Directory)
        );
    }
}