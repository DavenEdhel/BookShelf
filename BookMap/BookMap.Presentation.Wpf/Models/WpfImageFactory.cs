using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using BookMap.Core.Models;
using BookMap.Presentation.Wpf.InteractionModels;

namespace BookMap.Presentation.Wpf.Models
{
    public class WpfImageFactory : IImageFactory
    {
        private int w = 2560;
        private int h = 1920;

        public IImage MakeEmpty(string color)
        {
            var wb = new WriteableBitmap(w, h, 96, 96, PixelFormats.Bgra32, null);
            var pixels = new uint[w * h];
            for (var i = 0; i < pixels.Length; i++)
            {
                pixels[i] = new BgraColorFromHex(color).Bgra;
            }
            wb.WritePixels(new Int32Rect(0, 0, w, h), pixels, 4 * w, 0);

            return new WpfImage()
            {
                Value = wb
            };
        }

        public IImage MakeEmpty()
        {
            return new WpfImage()
            {
                Value = new WriteableBitmap(w, h, 96, 96, PixelFormats.Bgra32, null)
            };
        }

        public IImage LoadFrom(string path)
        {
            return new WpfImage
            {
                Value = new WriteableBitmap(new BitmapImage(new Uri(path)))
            };
        }

        public IImage LoadNull()
        {
            return new WpfImage();
        }
    }
}