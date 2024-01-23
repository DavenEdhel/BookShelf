using System;
using BookMap.Presentation.Apple.Models;

namespace BookMap.Presentation.Apple.Services
{
    public class BookmarkDto
    {
        public FrameDouble World { get; set; }

        public string Name { get; set; }
    }

    public class BookmarkV2
    {
        public Guid Id { get; set; }

        public FrameDouble World { get; set; }

        public string Name { get; set; }

        public string Payload { get; set; } = string.Empty;
    }
}