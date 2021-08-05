using BookMap.Presentation.Apple.Extentions;
using UIKit;

namespace BookMap.Presentation.Apple.Services
{
    public static class BrushInfoExtensions
    {
        public static BrushInfo ToBrushInfoWithColor(this float size, UIColor color)
        {
            return new BrushInfo(size, color.ToHexString());
        }
    }
}