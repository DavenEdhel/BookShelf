using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using BookMap.Core.Models;
using BookMap.Presentation.Apple.Models;
using BookMap.Presentation.Apple.Services;
using BookMap.Presentation.Wpf.InteractionModels;
using BookMap.Presentation.Wpf.MapModels.DrawModels;
using BookMap.Presentation.Wpf.Models;

namespace BookMap.Presentation.Wpf.MapModels
{
    public class MapPart : Image, IMapPart
    {
        private readonly MapReference _reference;
        private readonly bool _isLabel;
        private readonly MapProviderSynchronous _mapProvider;
        private int w = 2560;
        private int h = 1920;
        private WriteableBitmap _source;
        private IImage _image;

        public MapPart(MapReference reference, bool isLabel, MapProviderSynchronous mapProvider)
        {
            _reference = reference;
            _isLabel = isLabel;
            _mapProvider = mapProvider;
        }

        public ImagePosition Position { get; private set; }

        public void Load(ImagePosition position)
        {
            Position = position;

            _image = _mapProvider.GetImageAsync(position, _isLabel);

            Source = _source = _image.ToWpfImage().Value;

            //Source = _source = new WriteableBitmap(
            //    new BitmapImage(
            //        new Uri(
            //            new MapPartFilePath(_info, position, _isLabel).FullPath
            //        )
            //    )
            //);
        }

        public bool Contains(Point point)
        {
            if (Opacity < 0.99 || Source == null)
            {
                return false;
            }

            var leftTop = new Point(Canvas.GetLeft(this), Canvas.GetTop(this));
            var absolute = point;
            if (absolute.X > leftTop.X &&
                absolute.Y > leftTop.Y &&
                absolute.X < leftTop.X + Width &&
                absolute.Y < leftTop.Y + Height)
            {
                return true;
            }

            return false;
        }

        public void Draw(Point p3, IBrush brush)
        {
            var p4 = new Point()
            {
                X = p3.X * w / Width - brush.SizeInPixels/2,
                Y = p3.Y * h / Height - brush.SizeInPixels/2
            };

            var writable = (WriteableBitmap)Source;

            writable.Draw(p4, brush, new Circle());

            //var area = new Int32Rect((int)p4.X, (int)p4.Y, 10, 10);
            //var part = new uint[10 * 10];
            //var stride = 10 * 4;
            //var offset = 0;

            //writable.CopyPixels(area, part, stride, offset);

            //for (var j = 0; j < 100; j++)
            //{
            //    part[j] = 4292381556;
            //}

            //writable.WritePixels(area, part, stride, offset);
        }

        public void Save()
        {
            _image.TrySave(new MapPartFilePath(_reference, Position, _isLabel).FullPath);
        }
    }
}