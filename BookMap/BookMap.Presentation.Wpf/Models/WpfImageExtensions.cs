using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using BookMap.Core.Models;

namespace BookMap.Presentation.Wpf.Models
{
    public static class WpfImageExtensions
    {
        public static BitmapImage ToUIImage(this IImage image)
        {
            var bitmap = ((WpfImage) image).Value;

            return bitmap;
        }
    }
}