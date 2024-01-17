using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BookMap.Core.Models;
using BookMap.Presentation.Apple.Models;
using BookMap.Presentation.Apple.Services;
using BookMap.Presentation.Wpf.InteractionModels;
using BookMap.Presentation.Wpf.MapModels.DrawModels;
using BookMap.Presentation.Wpf.Models;

namespace BookMap.Presentation.Wpf.MapModels
{
    public class TextAsTexture
    {
        public Size Size { get; set; }

        public uint[] Buffer { get; set; }

        public double FontSize { get; set; }
    }

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

        public double GetScaleFactorW()
        {
            return w / Width;
        }

        public double GetScaleFactorH()
        {
            return h / Height;
        }

        public DrawingResult Draw(Point p3, IBrush brush)
        {
            var p4 = new Point()
            {
                X = p3.X * w / Width - brush.SizeInPixels/2,
                Y = p3.Y * h / Height - brush.SizeInPixels/2
            };

            var writable = (WriteableBitmap)Source;

            return writable.Draw(p4, brush, new Circle());
        }

        public TextAsTexture RenderTextIntoMemory(string text, Color color, TextBox tb)
        {
            var fontSize = tb.FontSize * GetScaleFactorH();
            var height = tb.ActualHeight * GetScaleFactorH();
            var width = tb.ActualWidth * GetScaleFactorW();

            if (Double.IsNaN(fontSize) || Double.IsNaN(height) || Double.IsNaN(width))
            {
                return null;
            }

            var writeableBm1 = (WriteableBitmap)Source;

            var tt = new FormattedText(
                text,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(tb.FontFamily, tb.FontStyle, tb.FontWeight, tb.FontStretch),
                fontSize,
                new SolidColorBrush(color),
                new NumberSubstitution(),
                TextFormattingMode.Ideal,
                VisualTreeHelper.GetDpi(this).PixelsPerDip
            );

            DrawingVisual drawingVisual = new DrawingVisual();
            DrawingContext drawingContext = drawingVisual.RenderOpen();
            drawingContext.DrawText(tt, new Point(0, 0));
            drawingContext.Close();

            RenderTargetBitmap bmp = new RenderTargetBitmap((int)width, (int)height, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(drawingVisual);

            var area = new Int32Rect(0, 0, bmp.PixelWidth, bmp.PixelHeight);
            var part = new uint[bmp.PixelWidth * bmp.PixelHeight];
            var stride = bmp.PixelWidth * 4;

            bmp.CopyPixels(area, part, stride, 0);

            return new TextAsTexture()
            {
                Size = new Size(bmp.PixelWidth, bmp.PixelHeight),
                Buffer = part,
                FontSize = fontSize
            };
        }

        public DrawingResult Draw(Point p3, TextAsTexture texture)
        {
            var p4 = new Point()
            {
                X = p3.X * w / Width,
                Y = p3.Y * h / Height - texture.FontSize / 2
            };

            var writeableBm1 = (WriteableBitmap)Source;

            return writeableBm1.Draw(p4, texture.Size, texture.Buffer);
        }

        public void Draw(Point p3, string text, Color color, TextBox tb)
        {
            var texture = RenderTextIntoMemory(text, color, tb);

            if (texture == null)
            {
                return;
            }

            var p4 = new Point()
            {
                X = p3.X * w / Width,
                Y = p3.Y * h / Height - texture.FontSize/ 2
            };

            var writeableBm1 = (WriteableBitmap)Source;

            writeableBm1.Draw(p4, texture.Size, texture.Buffer);
        }

        public void Save()
        {
            _image.TrySave(new MapPartFilePath(_reference, Position, _isLabel).FullPath);
        }
    }
}