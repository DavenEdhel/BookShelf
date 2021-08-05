using System;
using System.Drawing;
using BookMap.Presentation.Apple.Models;
using CoreGraphics;
using UIKit;

namespace BookMap.Presentation.Apple.Extentions
{
    public static class ImageHelper
    {
        public static CGSize ImageSize => new CGSize(2560f, 1920f);

        public static string GetImageName(ImagePosition position, bool isLabel)
        {
            return $"{(isLabel ? "label" : "ground")}_{position.Level}_{position.X}_{position.Y}.png";
        }

        public static ImagePosition ExtractPositionForGround(this string mapName)
        {
            if (mapName.StartsWith("ground"))
            {
                var parts = mapName.Split('_');
                var lastPart = parts[3].Split('.')[0];

                return new ImagePosition()
                {
                    Level = int.Parse(parts[1]),
                    X = int.Parse(parts[2]),
                    Y = Int32.Parse(lastPart)
                };
            }

            return null;
        }

        public static UIImage MakeEmptyImage(this UIColor color)
        {
            UIGraphics.BeginImageContext(new CGSize(2560f, 1920f));

            var context = UIGraphics.GetCurrentContext();

            context.SetFillColor(color.CGColor);
            context.FillRect(new CGRect(0, 0, 2560, 1920));

            var raw = UIGraphics.GetImageFromCurrentImageContext();

            context.Dispose();

            UIGraphics.EndImageContext();

            return raw;
        }

        public static UIImage MakeEmptyImage()
        {
            UIGraphics.BeginImageContext(new CGSize(2560f, 1920f));

            var context = UIGraphics.GetCurrentContext();

            var raw = UIGraphics.GetImageFromCurrentImageContext();

            context.Dispose();

            UIGraphics.EndImageContext();

            return raw;
        }

        public static UIImage Crop(this UIImage self, CGRect frame, CGSize newSize)
        {
            UIGraphics.BeginImageContext(frame.Size);

            var context = UIGraphics.GetCurrentContext();

            context.TranslateCTM(0f, (nfloat)frame.Height);
            context.ScaleCTM(1.0f, -1.0f);

            context.DrawImage(new RectangleF(-(float)frame.X, -(float)frame.Y, self.CGImage.Width, self.CGImage.Height), self.CGImage);

            context.ScaleCTM(1.0f, -1.0f);
            context.TranslateCTM(0f, -(nfloat)frame.Height);

            var raw = UIGraphics.GetImageFromCurrentImageContext();

            var result = raw.Scale(newSize);

            raw.Dispose();

            context.Dispose();

            UIGraphics.EndImageContext();

            return result;
        }
    }
}