using BookMap.Core.Models;
using UIKit;

namespace BookMap.Presentation.Apple.Services
{
    public static class IosImageExtensions
    {
        public static IImage ToImage(this UIImage image) => new IosImage()
        {
            Value = image
        };

        public static UIImage ToUIImage(this IImage image) => ((IosImage) image).Value;
    }
}