using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using BookMap.Core.Models;
using BookMap.Presentation.Apple.Models;

namespace BookMap.Presentation.Wpf.Models
{
    public class WpfImage : IImage
    {
        public BitmapSource Value { get; set; }

        public IImage Copy()
        {
            return new WpfImage()
            {
                Value = Value.Clone()
            };
        }

        public bool EqualsTo(IImage oldImage)
        {
            return oldImage is WpfImage img &&
                   img.Value == Value;
        }

        public (bool, string) TrySave(string path)
        {
            throw new System.NotImplementedException();
        }

        public void Dispose()
        {
        }

        public IImage MakeSubImage(FrameDouble frameDouble)
        {
            var croppedImage = new CroppedBitmap(Value, new Int32Rect((int) frameDouble.X, 1920 - (int) frameDouble.Y - 240, (int) frameDouble.Width, (int) frameDouble.Height));

            var transformed = new TransformedBitmap(croppedImage, new ScaleTransform(8, 8));

            return new WpfImage()
            {
                Value = transformed
            };
        }
    }
}