﻿using System.Windows;

namespace BookWiki.Presentation.Wpf.Models
{
    public class ScreensStateDto
    {
        public string ScreenRelativePath { get; set; }

        public Point Position { get; set; }

        public bool WasMinimized { get; set; }

        public bool WasMaximized { get; set; }

        public double ScrollOffset { get; set; }
    }
}