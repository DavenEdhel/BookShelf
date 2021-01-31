using BookWiki.Core;
using BookWiki.Core.Files.FileSystemModels;
using BookWiki.Core.Files.PathModels;
using BookWiki.Presentation.Wpf.Models;
using BookWiki.Presentation.Wpf.Models.SpellCheckModels;

namespace BookWiki.Presentation.Wpf
{
    public class BookShelf
    {
        public static readonly BookShelf Instance = new BookShelf();

        public IFileSystemNode Root { get; }

        public IRootPath RootPath { get; }

        public IMutableWordCollection Dictionary { get; }

        public BookShelf()
        {
            RootPath = new RootPath();

            Root = new FileSystemNode(RootPath.FullPath);

            var lex = new WordCollectionFromLex(new FolderPath("Russian.lex").AbsolutePath(RootPath).FullPath);
            lex.Load();

            Dictionary = lex;
        }
    }
}