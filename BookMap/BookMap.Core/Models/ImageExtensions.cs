using BookMap.Presentation.Apple.Models;

namespace BookMap.Core.Models
{
    public static class ImageExtensions
    {
        public static string GetImageName(this ImagePosition position, bool isLabel)
        {
            return $"{(isLabel ? "label" : "ground")}_{position.Level}_{position.X}_{position.Y}.png";
        }
    }
}