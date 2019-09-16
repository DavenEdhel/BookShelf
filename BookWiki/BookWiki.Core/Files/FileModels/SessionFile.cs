using System.Collections.Generic;
using System.Linq;
using BookWiki.Core.Files.PathModels;
using BookWiki.Core.FileSystem.FileModels;
using BookWiki.Core.Utils.PropertyModels;
using BookWiki.Core.ViewModels;
using Newtonsoft.Json;

namespace BookWiki.Core.Files.FileModels
{
    public class SessionFile
    {
        private readonly IProperty<UserInterfaceSettings> _settings;
        private readonly IProperty<IEnumerable<string>> _queries;
        private readonly IProperty<IEnumerable<IEditorState>> _contentPaths;
        private readonly IAbsolutePath _path;
        private IFile _file;

        public IEnumerable<string> Queries => _queries.Value;
        public IEnumerable<IEditorState> States => _contentPaths.Value;

        public UserInterfaceSettings Settings => _settings.Value;

        public SessionFile(IEnumerable<string> queries, IEnumerable<IEditorState> contentPaths, UserInterfaceSettings settings, IAbsolutePath folderPath)
        {
            _settings = new CachedValue<UserInterfaceSettings>(settings);
            _queries = new CachedValue<IEnumerable<string>>(queries);
            _contentPaths = new CachedValue<IEnumerable<IEditorState>>(contentPaths);
            _path = new FilePath(folderPath, "Session.json");
            _file = new TextFile(_path);
        }

        public SessionFile(IAbsolutePath folderPath)
        {
            _queries = new CachedValue<IEnumerable<string>>(() =>
            {
                var content = _file.Content;

                var dto = JsonConvert.DeserializeObject<SessionFileStructure>(content) ?? new SessionFileStructure();

                return dto.Queries;
            });

            _contentPaths = new CachedValue<IEnumerable<IEditorState>>(() =>
            {
                var content = _file.Content;

                var dto = JsonConvert.DeserializeObject<SessionFileStructure>(content) ?? new SessionFileStructure();

                return dto.States.Select(EditorState.Create);
            });

            _settings = new CachedValue<UserInterfaceSettings>(() =>
            {
                var content = _file.Content;

                var dto = JsonConvert.DeserializeObject<SessionFileStructure>(content) ?? new SessionFileStructure();

                return dto.Settings;
            });

            _path = new FilePath(folderPath, "Session.json");
            _file = new TextFile(_path);
        }

        public void Save()
        {
            var dto = new SessionFileStructure()
            {
                Settings = _settings.Value,
                States = States.Select(x => x.ToJson()).ToList(),
                Queries = Queries.ToList()
            };

            var content = JsonConvert.SerializeObject(dto);

            _file.Save(content);
        }

        private class SessionFileStructure
        {
            public UserInterfaceSettings Settings { get; set; } = new UserInterfaceSettings();

            public List<string> Queries { get; set; } = new List<string>();

            public List<string> States { get; set; } = new List<string>();
        }
    }
}