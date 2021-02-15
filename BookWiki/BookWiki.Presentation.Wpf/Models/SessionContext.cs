using System.Collections.Generic;
using System.IO;
using System.Linq;
using BookWiki.Core;
using BookWiki.Core.Files.FileModels;
using BookWiki.Core.Files.PathModels;
using BookWiki.Core.LibraryModels;
using BookWiki.Core.Search;
using BookWiki.Core.ViewModels;
using Newtonsoft.Json;

namespace BookWiki.Presentation.Wpf.Models
{
    public class SessionContext : ISessionContext
    {
        private readonly IRootPath _path;
        private SessionFile _file;

        public SessionContext(IRootPath path)
        {
            _path = path;
            _file = new SessionFile(path);
        }

        public IEnumerable<IEditorState> OpenedContentTabs { get; private set; }

        public IEnumerable<IQuery> OpenedQueriesTabs { get; private set; }

        public UserInterfaceSettings InterfaceSettings { get; private set; }

        public IEnumerable<ScreenState> ScreenStates { get; private set; }

        private string StatesFilePath => Path.Combine(_path.FullPath, "WindowsStates.json");

        public SessionContext Restore()
        {
            OpenedContentTabs = _file.States;
            OpenedQueriesTabs = _file.Queries.Select(x => new SearchQuery(null, x)).ToArray();
            InterfaceSettings = _file.Settings;

            if (File.Exists(StatesFilePath) == false)
            {
                ScreenStates = new ScreenState[0];
            }
            else
            {
                ScreenStates = JsonConvert.DeserializeObject<ScreensStateDto[]>(File.ReadAllText(StatesFilePath)).Select(x => new ScreenState(x));
            }

            return this;
        }

        public void Store(IEnumerable<NovelWindow> openedNovels, PageConfig pageConfig, IRootPath root)
        {
            var file = new SessionFile(new List<string>(), openedNovels.Select(x => new EditorState(x)).ToArray(), new UserInterfaceSettings()
            {
                IsSideBarHidden = pageConfig.Current.IsSideBarHidden,
                IsScrollHidden = pageConfig.Current.IsScrollHidden,
                PageModeIndex = pageConfig.Current.PageModeIndex,
                IsSpellCheckOn = pageConfig.Current.IsSpellCheckOn
            }, root);

            file.Save();

            File.WriteAllText(StatesFilePath, JsonConvert.SerializeObject(openedNovels.Select(x => new ScreenState(x).ToDto()).ToArray()));
        }
    }
}