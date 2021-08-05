using System;
using BookMap.Core.Models;
using BookMap.Presentation.Apple.Extentions;
using BookMap.Presentation.Apple.Models;
using CoreGraphics;
using Foundation;
using UIKit;

namespace BookMap.Presentation.Apple.Services
{
    public class IosImage : IImage
    {
        public UIImage Value { get; set; }

        public IImage Copy()
        {
            return new IosImage()
            {
                Value = UIImage.FromImage(Value.CGImage)
            };
        }

        public bool EqualsTo(IImage oldImage)
        {
            return oldImage is IosImage iosImage &&
                   iosImage.Value == Value;
        }

        public (bool, string) TrySave(string path)
        {
            var png = Value.AsPNG();

            NSError error;

            if (png.Save(path, false, out error) == false)
            {
                return (false, error.LocalizedDescription);
            }

            return (true, string.Empty);
        }

        public void Dispose()
        {
            Value.Dispose();
        }

        public IImage MakeSubImage(FrameDouble frame)
        {
            UIGraphics.BeginImageContext(frame.ToSize());
            //UIGraphics.BeginImageContext(upper.Size);

            var context = UIGraphics.GetCurrentContext();

            context.TranslateCTM(0f, (nfloat)frame.Height);
            context.ScaleCTM(1.0f, -1.0f);

            context.DrawImage(new CGRect(-(float)frame.X, -(float)frame.Y, Value.CGImage.Width, Value.CGImage.Height), Value.CGImage);
            
            context.ScaleCTM(1.0f, -1.0f);
            context.TranslateCTM(0f, -(nfloat)frame.Height);

            var raw = UIGraphics.GetImageFromCurrentImageContext();

            var result = raw.Scale(Value.Size);

            raw.Dispose();

            context.Dispose();

            UIGraphics.EndImageContext();

            return new IosImage()
            {
                Value = result
            };
        }
    }
}