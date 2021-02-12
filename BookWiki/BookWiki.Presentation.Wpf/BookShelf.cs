using System.IO;
using BookWiki.Core;
using BookWiki.Core.Files.FileSystemModels;
using BookWiki.Core.Files.PathModels;
using BookWiki.Presentation.Wpf.Models;
using BookWiki.Presentation.Wpf.Models.SpellCheckModels;
using Newtonsoft.Json;

namespace BookWiki.Presentation.Wpf
{
    public class BookShelf
    {
        public static readonly BookShelf Instance = new BookShelf();

        public IFileSystemNode Root { get; }

        public IRootPath RootPath { get; }

        public IMutableWordCollection Dictionary { get; }

        public AppConfigDto Config { get; }

        public BookShelf()
        {
            Config = JsonConvert.DeserializeObject<AppConfigDto>(File.ReadAllText("AppConfig.json"));

            RootPath = new RootPath(Config.Root);

            Root = new FileSystemNode(RootPath.FullPath);

            var lex = new WordCollectionFromLex(new FolderPath("Russian.lex").AbsolutePath(new RootPath(Config.LibraryPath)).FullPath);
            lex.Load();

            Dictionary = lex;
        }
    }
}