using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using BookMap.Core.Models;

namespace BookMap.Presentation.Wpf.Models
{
    public class WpfImageFactory : IImageFactory
    {
        private int w = 2560;
        private int h = 1920;

        public IImage MakeEmpty(string color)
        {
            return new WpfImage()
            {
                Value = new WriteableBitmap(w, h, 96, 96, PixelFormats.Bgra32, null)
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