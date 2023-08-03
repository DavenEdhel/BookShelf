using System.IO;
using BookMap.Presentation.Wpf.Models;

namespace BookMap.Presentation.Wpf.MapModels
{
    public class MapReference
    {
        private readonly AppConfigDto _config;
        private readonly string _mapName;

        public MapReference(AppConfigDto config, string mapName)
        {
            _config = config;
            _mapName = mapName;
        }

        public string FullPath => Path.Combine(_config.Root, _mapName);
    }
}