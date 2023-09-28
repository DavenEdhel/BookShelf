using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using BookWiki.Core;
using BookWiki.Core.Articles;
using BookWiki.Core.Files.FileModels;
using BookWiki.Core.Files.FileSystemModels;
using BookWiki.Core.Files.PathModels;
using BookWiki.Presentation.Wpf.Extensions;
using BookWiki.Presentation.Wpf.Models;
using BookWiki.Presentation.Wpf.Models.QuickNavigationModels;
using BookWiki.Presentation.Wpf.Models.SpellCheckModels;
using Newtonsoft.Json;
using Exception = System.Exception;
using BookWiki.Presentation.Wpf.Models.KeyProcessorModels;

namespace BookWiki.Presentation.Wpf
{
    public class BookShelf
    {
        public static readonly BookShelf Instance = new BookShelf();
        private bool _sessionRestored;

        public IEnumerable<NovelWindow> OpenedNovels
        {
            get
            {
                foreach (Window currentWindow in Application.Current.Windows)
                {
                    if (currentWindow is NovelWindow novelWindow)
                    {
                        yield return novelWindow;
                    }
                }
            }
        }

        public IEnumerable<ArticleWindow> OpenedArticles
        {
            get
            {
                foreach (Window currentWindow in Application.Current.Windows)
                {
                    if (currentWindow is ArticleWindow articleWindow)
                    {
                        yield return articleWindow;
                    }
                }
            }
        }

        public AllArticlesWindow AllArticlesWindow
        {
            get
            {
                foreach (Window currentWindow in Application.Current.Windows)
                {
                    if (currentWindow is AllArticlesWindow fs)
                    {
                        return fs;
                    }
                }

                return null;
            }
        }

        public FileSystemWindow FileSystemWindow
        {
            get
            {
                foreach (Window currentWindow in Application.Current.Windows)
                {
                    if (currentWindow is FileSystemWindow fs)
                    {
                        return fs;
                    }
                }

                return null;
            }
        }

        public BookShelf()
        {
            Config = JsonConvert.DeserializeObject<AppConfigDto>(File.ReadAllText("AppConfig.json"));

            RootPath = new RootPath(Config.Root);

            Root = new FileSystemNode(RootPath.FullPath);

            var lex = new WordCollectionFromLex(new FolderPath("Russian.lex").AbsolutePath(new RootPath(Config.LibraryPath)).FullPath);
            lex.Load();

            Dictionary = lex;

            Session = new SessionContext(RootPath).Restore();

            PageConfig = new PageConfig(Session);
            RightSideBarConfig = new RightSideBarConfig(Session);

            Articles = new ArticlesLibrary(RootPath);

            Trashcan = new Trashcan(RootPath);
        }

        public KeyProcessor KeyProcessor { get; } = new KeyProcessor();

        public SearchByFileEngine Search { get; } = new SearchByFileEngine();

        public ArticlesLibrary Articles { get; }

        public Trashcan Trashcan { get; }

        public SessionContext Session { get; }

        public IFileSystemNode Root { get; }

        public IRootPath RootPath { get; }

        public IMutableWordCollection Dictionary { get; }

        public AppConfigDto Config { get; }

        public IEnumerable<TabDto> Tabs
        {
            get
            {
                foreach (var configTab in Config.Tabs)
                {
                    if (Directory.Exists(new FolderPath(configTab.Path).AbsolutePath(RootPath).FullPath))
                    {
                        yield return configTab;
                    }
                }
            }
        }

        public PageConfig PageConfig { get; }

        public RightSideBarConfig RightSideBarConfig { get; }

        public IRelativePath CurrentNovel { get; private set; }

        public event Action ItemsListChanged = delegate { };

        public void RestoreLastSession()
        {
            if (_sessionRestored)
            {
                return;
            }

            foreach (var sessionOpenedContentTab in Session.OpenedContentTabs)
            {
                Open(sessionOpenedContentTab.NovelPathToLoad);
            }

            _sessionRestored = true;
        }

        public void Save(IFileSystemNode parentNode, FileName fileName, IExtension extension)
        {
            var node = new FileSystemNode(parentNode, new FolderPath(parentNode.Path, fileName, extension));
            node.SaveUnder(parentNode);

            Search.InvalidateCache();
        }

        public Novel Read(IRelativePath novel)
        {
            Novel novelItem;

            novelItem = new Novel(novel, BookShelf.Instance.RootPath);
            return novelItem;
        }

        public void OpenArticlesSearch(string search = "", bool focusedOnQuery = false)
        {
            foreach (Window currentWindow in Application.Current.Windows)
            {
                if (currentWindow is AllArticlesWindow fs)
                {
                    if (currentWindow.WindowState == WindowState.Minimized)
                    {
                        currentWindow.WindowState = WindowState.Normal;
                    }

                    fs.Activate();

                    fs.SetQuery(search);

                    if (focusedOnQuery)
                    {
                        fs.FocusOnQuery();
                    }

                    return;
                }
            }

            var aw = new AllArticlesWindow(search);
            
            aw.Show();

            if (focusedOnQuery)
            {
                aw.FocusOnQuery();
            }

            ItemsListChanged();
        }

        public void OpenArticleOrSearch(string query)
        {
            var searchResult = Articles.Search($"!{query.Trim().ToLowerInvariant()}");
            if (searchResult.Count() == 1)
            {
                OpenArticle(searchResult.First().Article.Source);
            }
            else if (searchResult.Count() > 1)
            {
                OpenArticlesSearch($"!{query.Trim().ToLowerInvariant()}", true);
            }
            else
            {
                OpenArticlesSearch($"{query.Trim()}", focusedOnQuery: true);
            }
        }

        public ArticleWindow OpenArticle(IRelativePath novel, bool fullscreen = true)
        {
            var articleView = OpenedArticles.FirstOrDefault(x => x.Novel.EqualsTo(novel));

            if (articleView != null)
            {
                articleView.ActivateOrRestore(fullscreen);

                return articleView;
            }
            else
            {
                Article articleItem;

                try
                {
                    articleItem = Articles.Get(novel);
                    var novelLength = articleItem.Content.Length;
                }
                catch (Exception e)
                {
                    MessageBox.Show($"Cannot restore {novel.Name}.", e.ToString());

                    return null;
                }

                var wnd = new ArticleWindow(articleItem);

                var state = Session.ScreenStates.FirstOrDefault(x => x.Novel.EqualsTo(novel));

                if (state != null)
                {
                    state.ApplyTo(wnd);
                }

                if (fullscreen)
                {
                    wnd.WindowState = WindowState.Maximized;
                }

                wnd.Show();

                ItemsListChanged();

                return wnd;
            }
        }

        public void Open(IRelativePath novel, bool fullscreen = false)
        {
            CurrentNovel = novel;

            var novelView = OpenedNovels.FirstOrDefault(x => x.Novel.EqualsTo(novel));

            if (novelView != null)
            {
                novelView.ActivateOrRestore(fullscreen);
            }
            else
            {
                Novel novelItem;

                try
                {
                    novelItem = new Novel(novel, BookShelf.Instance.RootPath);
                    var novelLength = novelItem.Content.Length;
                }
                catch (Exception e)
                {
                    MessageBox.Show($"Cannot restore {novel.Name}.", e.ToString());

                    return;
                }

                var wnd = new NovelWindow(novelItem);

                var state = Session.ScreenStates.FirstOrDefault(x => x.Novel.EqualsTo(novel));

                if (state != null)
                {
                    state.ApplyTo(wnd);
                }

                if (fullscreen)
                {
                    wnd.WindowState = WindowState.Maximized;
                }

                wnd.Show();

                ItemsListChanged();
            }
        }

        public async void CloseAllAsync()
        {
            AllArticlesWindow?.Close();

            foreach (var openedArticles in OpenedArticles)
            {
                openedArticles.Close();
            }

            foreach (var openedNovel in OpenedNovels)
            {
                openedNovel.Close();
            }

            for (int i = 0; i < 100; i++)
            {
                await Task.Delay(100);

                if (OpenedNovels.Any(x => x.ClosingFailed))
                {
                    FileSystemWindow.Restore();

                    return;
                }
                
                if (OpenedNovels.Any() == false)
                {
                    FileSystemWindow.Close();

                    return;
                }
            }

            ItemsListChanged();
        }

        public void StoreSession()
        {
            Session.Store(OpenedNovels, PageConfig, RightSideBarConfig, RootPath);
        }

        public void ShowFileSystem()
        {
            foreach (Window currentWindow in Application.Current.Windows)
            {
                if (currentWindow is FileSystemWindow fs)
                {
                    if (currentWindow.WindowState == WindowState.Minimized)
                    {
                        currentWindow.WindowState = WindowState.Normal;
                    }

                    fs.Activate();
                }
            }
        }

        public void ReportWindowClosed()
        {
            ItemsListChanged();
        }
    }
}