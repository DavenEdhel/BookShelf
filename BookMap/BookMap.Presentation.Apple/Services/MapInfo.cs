using System.Collections.Generic;
using UIKit;

namespace BookMap.Presentation.Apple.Services
{
    public class MapInfo
    {
        public double Width { get; set; } = 100;

        public List<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();

        public BrushInfo[] Brushes { get; set; } = new BrushInfo[8]
        {
            new BrushInfo(10, UIColor.Green),
            new BrushInfo(3, UIColor.Black),
            new BrushInfo(1, UIColor.White),
            new BrushInfo(20, UIColor.Blue),
            new BrushInfo(10, UIColor.Green),
            new BrushInfo(3, UIColor.Black),
            new BrushInfo(1, UIColor.White),
            new BrushInfo(10, UIColor.Green)
        };

        public bool ShowPalette { get; set; } = true;

        public bool ShowBookmarks { get; set; } = true;
    }
}