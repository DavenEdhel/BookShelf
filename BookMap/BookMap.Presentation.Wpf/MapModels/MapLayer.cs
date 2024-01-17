using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BookMap.Presentation.Apple.Models;
using BookMap.Presentation.Wpf.InteractionModels;
using BookMap.Presentation.Wpf.MapModels.DrawModels;

namespace BookMap.Presentation.Wpf.MapModels
{
    public class MapLayer
    {
        private readonly Canvas _container;
        private readonly MapPart[] _parts;
        private readonly List<MapPart> _affectedParts = new();
        private readonly List<(ImagePosition, Point)> _snappedPoints = new();

        public MapLayer(
            Canvas container,
            MapPart[] parts)
        {
            _container = container;
            _parts = parts;
        }

        public MapPart MapPart(Point point)
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

        public MapPart MapPart(ImagePosition position)
        {
            return _parts.Where(x => x.Position != null).FirstOrDefault(x => x.Position.EqualTo(position));
        }

        public void SnapshotPositionsForMaps(MouseEventArgs point)
        {
            var absolute = point.GetPosition(_container);

            var current = MapPart(absolute);

            var p3 = point.GetPosition(current);

            _snappedPoints.Clear();

            _snappedPoints.Add((current.Position, p3));

            foreach (var affectedPart in AffectedParts(current, new DrawingResult()
                     {
                         CroppedTop = true,
                         CroppedBottom = true,
                         CroppedLeft = true,
                         CroppedRight = true
                     }).Where(x => x is not null && x.Position != null))
            {
                _snappedPoints.Add((affectedPart.Position, point.GetPosition(affectedPart)));
            }

        }

        public IEnumerable<MapPart> AffectedParts(MapPart current, DrawingResult result)
        {
            if (result.CroppedTop)
            {
                yield return MapPart(current.Position.Up());
            }

            if (result.CroppedTop && result.CroppedRight)
            {
                yield return MapPart(current.Position.Up().Right());
            }

            if (result.CroppedRight)
            {
                yield return MapPart(current.Position.Right());
            }

            if (result.CroppedRight && result.CroppedBottom)
            {
                yield return MapPart(current.Position.Right().Down());
            }

            if (result.CroppedBottom)
            {
                yield return MapPart(current.Position.Down());
            }

            if (result.CroppedBottom && result.CroppedLeft)
            {
                yield return MapPart(current.Position.Down().Left());
            }

            if (result.CroppedLeft)
            {
                yield return MapPart(current.Position.Left());
            }

            if (result.CroppedLeft && result.CroppedTop)
            {
                yield return MapPart(current.Position.Left().Up());
            }
        }

        public MapPart MapPart(MouseEventArgs point)
        {
            var absolute = point.GetPosition(_container);

            return MapPart(absolute);
        }

        public void Draw(MouseEventArgs point, IBrush brush)
        {
            var absolute = point.GetPosition(_container);

            var image = MapPart(absolute);

            if (image != null)
            {
                var p3 = point.GetPosition(image);

                var result = image.Draw(p3, brush);

                _affectedParts.Add(image);

                var affected = AffectedParts(image, result);

                if (affected.Any())
                {
                    foreach (var mapPart in affected)
                    {
                        var p4 = point.GetPosition(mapPart);
                        mapPart.Draw(p4, brush);
                        _affectedParts.Add(mapPart);
                    }
                }
            }
        }

        public void Draw(Point absolute, string text, IBrush brush, TextBox tb)
        {
            var image = MapPart(absolute);

            if (image != null)
            {
                var texture = image.RenderTextIntoMemory(text, new MediaColorFromArgbColor(new RgbaColorFromBgra(brush.Color.Bgra)).Color, tb);

                if (texture == null)
                {
                    return;
                }

                var result = image.Draw(_snappedPoints.First(x => x.Item1.EqualTo(image.Position)).Item2, texture);

                _affectedParts.Add(image);

                var affected = AffectedParts(image, result);

                if (affected.Any())
                {
                    foreach (var mapPart in affected)
                    {
                        var p4 = _snappedPoints.First(x => x.Item1.EqualTo(mapPart.Position)).Item2;
                        mapPart.Draw(p4, texture);
                        _affectedParts.Add(mapPart);
                    }
                }
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