using BookMap.Presentation.Apple.Models;

namespace BookMap.Core.Models
{
    public interface IImage
    {
        IImage Copy();

        bool EqualsTo(IImage oldImage);

        (bool, string) TrySave(string path);

        void Dispose();

        IImage MakeSubImage(FrameDouble frameDouble);
    }

    public interface IImageFactory
    {
        IImage MakeEmpty(string color);

        IImage MakeEmpty();

        IImage LoadFrom(string path);

        IImage LoadNull();
    }
}