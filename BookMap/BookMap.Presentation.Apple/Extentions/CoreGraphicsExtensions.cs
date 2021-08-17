using BookMap.Presentation.Apple.Models;
using CoreGraphics;

namespace BookMap.Presentation.Apple.Extentions
{
    public static class CoreGraphicsExtensions
    {
        public static PointDouble2D ToPoint(this CGPoint self)
        {
            return new PointDouble2D()
            {
                X = self.X,
                Y = self.Y
            };
        }

        public static BoundsDouble ToBounds(this CGSize self)
        {
            return new BoundsDouble()
            {
                Width = self.Width,
                Height = self.Height
            };
        }

        public static CGSize ToSize(this FrameDouble self)
        {
            return new CGSize(self.Width, self.Height);
        }

        public static CGRect ToRect(this FrameDouble self)
        {
            return new CGRect(self.X, self.Y, self.Width, self.Height);
        }
    }
}