using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BookMap.Presentation.Wpf.InteractionModels;

namespace BookMap.Presentation.Wpf.MapModels
{
    public class MapLayer
    {
        private readonly Canvas _container;
        private readonly MapPart[] _parts;
        private List<MapPart> _affectedParts = new List<MapPart>();

        public MapLayer(
            Canvas container,
            MapPart[] parts)
        {
            _container = container;
            _parts = parts;
        }

        private MapPart MapPart(Point point)
        {
            foreach (var mapPart in _parts)
            {
                if (mapPart.Contains(point))
                {
                    return mapPart;
                }
            }

            return null;
        }

        public void Draw(MouseEventArgs point, IBrush brush)
        {
            var absolute = point.GetPosition(_container);

            var image = MapPart(absolute);

            if (image != null)
            {
                var p3 = point.GetPosition(image);

                image.Draw(p3, brush);

                _affectedParts.Add(image);
            }
        }

        public void Save()
        {
            foreach (var mapPart in _affectedParts.Distinct())
            {
                mapPart.Save();
            }
        }
    }
}