using System;
using System.Collections.Generic;
using BookMap.Presentation.Apple.Models;

namespace BookMap.Presentation.Apple.Services
{
    public class MapInfo
    {
        public double Width { get; set; } = 100;

        public List<BookmarkDto> Bookmarks { get; set; } = new List<BookmarkDto>();

        public List<BookmarkV2> BookmarksV2 { get; set; } = new List<BookmarkV2>();

        public List<PinDto> Pins { get; set; } = new List<PinDto>();

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

        public Dictionary<string, PaletteDto> PalettesV2 { get; set; } = new Dictionary<string, PaletteDto>();

        public bool ShowPalette { get; set; } = true;

        public bool ShowBookmarks { get; set; } = true;
    }

    public class PaletteDto
    {
        public BrushInfo[] Brushes { get; set; } = new BrushInfo[0];

        public int Selected { get; set; } = 0;
    }

    public class PinDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Payload { get; set; }

        public ImagePositionDouble Position { get; set; }
    }
}