using BookWiki.Core.Files.FileSystemModels;
using BookWiki.Core.Files.PathModels;

namespace BookWiki.Core.Articles
{
    public class Trashcan
    {
        private readonly IRootPath _rootPath;

        public Trashcan(IRootPath rootPath)
        {
            _rootPath = rootPath;

            Root.EnsureCreated();
        }

        public FolderPath Root => new FolderPath(
            _rootPath,
            new FileName("Мусорка"),
            new Extension(NodeType.Directory)
        );
    }
}