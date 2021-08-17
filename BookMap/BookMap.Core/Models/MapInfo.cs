using System.Collections.Generic;

namespace BookMap.Presentation.Apple.Services
{
    public class MapInfo
    {
        public double Width { get; set; } = 100;

        public List<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();

        public BrushInfo[] Brushes { get; set; } = new BrushInfo[8]
        {
            new BrushInfo(10, "#00FF00"),
            new BrushInfo(3, "#000000"),
            new BrushInfo(1, "#FFFFFF"),
            new BrushInfo(20, "#FFFFFF"),
            new BrushInfo(10, "#FFFFFF"),
            new BrushInfo(3, "#FFFFFF"),
            new BrushInfo(1, "#FFFFFF"),
            new BrushInfo(10, "#FFFFFF")
        };

        public bool ShowPalette { get; set; } = true;

        public bool ShowBookmarks { get; set; } = true;
    }
}