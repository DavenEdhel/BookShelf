using System.Collections.Generic;
using System.Linq;
using BookWiki.Core.Files.PathModels;
using BookWiki.Core.FileSystem.FileModels;
using BookWiki.Core.Utils.PropertyModels;
using Newtonsoft.Json;

namespace BookWiki.Core.Files.FileModels
{
    public class SessionFile
    {
        private readonly IProperty<UserInterfaceSettings> _settings;
        private readonly IProperty<IEnumerable<string>> _queries;
        private readonly IProperty<IEnumerable<IPath>> _contentPaths;
        private readonly IPath _path;
        private IFile _file;

        public IEnumerable<string> Queries => _queries.Value;
        public IEnumerable<IPath> Contents => _contentPaths.Value;

        public UserInterfaceSettings Settings => _settings.Value;

        public SessionFile(IEnumerable<string> queries, IEnumerable<IPath> contentPaths, UserInterfaceSettings settings, IPath folderPath)
        {
            _settings = new CachedValue<UserInterfaceSettings>(settings);
            _queries = new CachedValue<IEnumerable<string>>(queries);
            _contentPaths = new CachedValue<IEnumerable<IPath>>(contentPaths);
            _path = new FilePath(folderPath, "Session.json");
            _file = new TextFile(_path);
        }

        public SessionFile(IPath folderPath)
        {
            _queries = new CachedValue<IEnumerable<string>>(() =>
            {
                var content = _file.Content;

                var dto = JsonConvert.DeserializeObject<SessionFileStructure>(content) ?? new SessionFileStructure();

                return dto.Queries;
            });

            _contentPaths = new CachedValue<IEnumerable<IPath>>(() =>
            {
                var content = _file.Content;

                var dto = JsonConvert.DeserializeObject<SessionFileStructure>(content) ?? new SessionFileStructure();

                return dto.Paths.Select(x => new FolderPath(x));
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
                Paths = Contents.Select(x => x.FullPath).ToList(),
                Queries = Queries.ToList()
            };

            var content = JsonConvert.SerializeObject(dto);

            _file.Save(content);
        }

        private class SessionFileStructure
        {
            public UserInterfaceSettings Settings { get; set; } = new UserInterfaceSettings();

            public List<string> Queries { get; set; } = new List<string>();

            public List<string> Paths { get; set; } = new List<string>();
        }
    }
}