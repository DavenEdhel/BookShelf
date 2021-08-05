using System;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using BookMap.Core.Models;

namespace BookMap.Presentation.Wpf.Models
{
    public class WpfImageFactory : IImageFactory
    {
        public IImage MakeEmpty(string color)
        {
            return new WpfImage()
            {
                Value = new BitmapImage()
            };
        }

        public IImage MakeEmpty()
        {
            return new WpfImage()
            {
                Value = new BitmapImage()
            };
        }

        public IImage LoadFrom(string path)
        {
            return new WpfImage
            {
                Value = new BitmapImage(new Uri(path))
            };
        }

        public IImage LoadNull()
        {
            return new WpfImage();
        }
    }
}