using System.Collections.Generic;
using System.Linq;
using BookMap.Presentation.Apple.Models;
using UIKit;

namespace BookMap.Presentation.Apple.Services
{
    public class ImageCacheItem
    {
        
    }

    public class ImageCache
    {
        private readonly IDictionary<int, ImageCacheItem> _cache = new Dictionary<int, ImageCacheItem>();

        public void Register(UIImage result, ImagePosition position, bool isLabel)
        {
            
        }
    }
}