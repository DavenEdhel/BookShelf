using System;
using BookMap.Core.Models;
using BookMap.Presentation.Apple.Extentions;
using UIKit;

namespace BookMap.Presentation.Apple.Services
{
    public class ImageFactory : IImageFactory
    {
        public IImage MakeEmpty()
        {
            return new IosImage()
            {
                Value = ImageHelper.MakeEmptyImage()
            };
        }

        public IImage MakeEmpty(string color)
        {
            return new IosImage()
            {
                Value = ImageHelper.MakeEmptyImage(color.ToUIColor())
            };
        }

        public IImage LoadFrom(string path)
        {
            try
            {
                var fileImage = UIImage.FromFile(path);

                var memoryImage = UIImage.FromImage(fileImage.CGImage);

                fileImage.Dispose();

                return new IosImage()
                {
                    Value = memoryImage
                };
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public IImage LoadNull()
        {
            return new IosImage();
        }
    }
}