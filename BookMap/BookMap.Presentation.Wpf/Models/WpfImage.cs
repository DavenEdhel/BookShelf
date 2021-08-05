using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using BookMap.Core.Models;
using BookMap.Presentation.Apple.Models;

namespace BookMap.Presentation.Wpf.Models
{
    public class WpfImage : IImage
    {
        public BitmapImage Value { get; set; }

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
            return new WpfImage()
            {
                Value = new BitmapImage()
            };
        }
    }
}