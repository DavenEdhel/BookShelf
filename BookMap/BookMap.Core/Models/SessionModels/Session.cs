using System;
using System.IO;
using Newtonsoft.Json;

namespace BookMap.Presentation.Apple.Models
{
    public class Session
    {
        private SessionDto _dto = new SessionDto();

        private string Path
        {
            get
            {
                var dirPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                var newPath = System.IO.Path.Combine(dirPath, "session.json");

                return newPath;
            }
        }

        public SessionDto Info => _dto;

        public void Load()
        {
            if (File.Exists(Path))
            {
                _dto = JsonConvert.DeserializeObject<SessionDto>(File.ReadAllText(Path));
            }
        }

        public void Save()
        {
            File.WriteAllText(Path, JsonConvert.SerializeObject(_dto));
        }
    }
}