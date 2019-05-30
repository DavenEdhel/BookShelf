using System.Collections.Generic;
using System.Linq;
using BookWiki.Core;
using BookWiki.Core.Files.FileModels;
using BookWiki.Core.Files.PathModels;
using BookWiki.Presentation.Apple.Models;
using BookWiki.Presentation.Apple.Views;
using BookWiki.Presentation.Apple.Views.Main;

namespace BookWiki.Presentation.Apple.Controllers
{
    public class SessionContext
    {
        private readonly ILibrary _library;

        private SessionFile _file;

        public IEnumerable<IPath> OpenedContentTabs { get; private set; }

        public IEnumerable<IQuery> OpenedQueriesTabs { get; private set; }

        public UserInterfaceSettings InterfaceSettings { get; private set; }

        public SessionContext(ILibrary library)
        {
            _library = library;
            _file = new SessionFile(new UserFolderPath());
        }

        public SessionContext Restore()
        {
            var ufp = new UserFolderPath();

            OpenedContentTabs = _file.Contents.Select(x => new PartsPath(ufp.Parts, x.Parts));
            OpenedQueriesTabs = _file.Queries.Select(x => new SearchQuery(_library, x)).ToArray();
            InterfaceSettings = _file.Settings;

            return this;
        }

        public void Store(TabsCollectionView tabs, ActionBarView actionBar)
        {
            var userFolder = new UserFolderPath();

            var contents = new List<IPath>();
            var queries = new List<IQuery>();

            foreach (var tab in tabs.OpenedCustomTabs)
            {
                if (tab.Data is IQuery query)
                {
                    queries.Add(query);
                }

                if (tab.Data is IContent content)
                {
                    var path = new PartsPath(content.Source.Parts.SkipWhile(x => x.PlainText != userFolder.Name.PlainText).Skip(1));

                    contents.Add(path);
                }
            }

            var file = new SessionFile(queries.Select(x => x.PlainText), contents, new UserInterfaceSettings() { IsSideBarHidden = actionBar.IsPanelHidden }, new UserFolderPath());

            file.Save();
        }
    }
}