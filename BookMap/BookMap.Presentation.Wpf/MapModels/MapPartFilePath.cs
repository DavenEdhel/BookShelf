using System.IO;
using BookMap.Presentation.Apple.Models;

namespace BookMap.Presentation.Wpf.MapModels
{
    public class MapPartFilePath
    {
        private readonly MapReference _reference;
        private readonly ImagePosition _position;
        private readonly bool _isLabel;

        public MapPartFilePath(MapReference reference, ImagePosition position, bool isLabel)
        {
            _reference = reference;
            _position = position;
            _isLabel = isLabel;
        }

        public string FullPath
        {
            get
            {
                return Path.Combine(_reference.FullPath, $"{new MapPartPrefixName(_isLabel).PrefixName}_{_position.Level}_{_position.X}_{_position.Y}.png");
            }
        }
    }
}