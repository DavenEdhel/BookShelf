using System.Collections.Generic;
using System.Linq;
using BookWiki.Core;
using BookWiki.Core.Files.FileModels;
using BookWiki.Core.Search;
using BookWiki.Core.ViewModels;
using BookWiki.Presentation.Apple.Models;
using BookWiki.Presentation.Apple.Views;
using BookWiki.Presentation.Apple.Views.Controls;
using BookWiki.Presentation.Apple.Views.Main;

namespace BookWiki.Presentation.Apple.Controllers
{
    public class SessionContext
    {
        private readonly ILibrary _library;

        private readonly SessionFile _file;

        public IEnumerable<IEditorState> OpenedContentTabs { get; private set; }

        public IEnumerable<IQuery> OpenedQueriesTabs { get; private set; }

        public UserInterfaceSettings InterfaceSettings { get; private set; }

        public SessionContext(ILibrary library)
        {
            _library = library;
            _file = new SessionFile(new UserFolderPath());
        }

        public SessionContext Restore()
        {
            OpenedContentTabs = _file.States;
            OpenedQueriesTabs = _file.Queries.Select(x => new SearchQuery(_library, x)).ToArray();
            InterfaceSettings = _file.Settings;

            return this;
        }

        public void Store(TabCollectionView tabCollection, ActionBarView actionBar, ContentHolderView contentHolder)
        {
            var contents = new List<IEditorState>();
            var queries = new List<IQuery>();

            foreach (var tab in tabCollection.OpenedCustomTabs)
            {
                if (tab.Data is IQuery query)
                {
                    queries.Add(query);
                }

                if (tab.Data is INovel novel)
                {
                    var openedTab = contentHolder.OpenedNovels.FirstOrDefault(x => x.Source.EqualsTo(novel.Source));

                    if (openedTab != null)
                    {
                        contents.Add(openedTab.State);
                    }
                    else
                    {
                        contents.Add(new MutableEditorState()
                        {
                            NovelPathToLoad = novel.Source
                        });
                    }
                }
            }

            var file = new SessionFile(queries.Select(x => x.PlainText), contents, new UserInterfaceSettings()
            {
                IsSideBarHidden = actionBar.PanelState.IsOff,
                IsScrollHidden = actionBar.ScrollState.IsOff,
                PageModeIndex = actionBar.PageMode.CurrentIndex
            }, new UserFolderPath());

            file.Save();
        }
    }
}