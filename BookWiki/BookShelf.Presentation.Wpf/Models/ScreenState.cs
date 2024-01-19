using System.Windows;
using BookShelf.Presentation.Wpf;
using BookWiki.Core.Files.PathModels;

namespace BookWiki.Presentation.Wpf.Models
{
    public class ScreenState
    {
        private readonly NovelWindow _novelWindow;

        public IRelativePath Novel { get; set; }

        public bool WasMinimized { get; set; }

        public bool WasMaximized { get; set; }

        public Point Position { get; set; }

        public double ScrollOffset { get; set; }

        public ScreenState(ScreensStateDto dto)
        {
            Novel = new FolderPath(dto.ScreenRelativePath);
            Position = dto.Position;
            WasMinimized = dto.WasMinimized;
            WasMaximized = dto.WasMaximized;
            ScrollOffset = dto.ScrollOffset;
        }

        public ScreenState(NovelWindow novelWindow)
        {
            _novelWindow = novelWindow;
        }

        public void ApplyTo(NovelWindow wnd)
        {
            wnd.Top = Position.Y;
            wnd.Left = Position.X;

            if (WasMinimized)
            {
                wnd.WindowState = WindowState.Minimized;
            }

            if (WasMaximized)
            {
                wnd.WindowState = WindowState.Maximized;
            }

            wnd.Scroll.ScrollToVerticalOffset(ScrollOffset);
        }

        public void ApplyTo(MapWindow wnd)
        {
            wnd.Top = Position.Y;
            wnd.Left = Position.X;

            if (WasMinimized)
            {
                wnd.WindowState = WindowState.Minimized;
            }

            if (WasMaximized)
            {
                wnd.WindowState = WindowState.Maximized;
            }
        }

        public void ApplyTo(ArticleWindow wnd)
        {
            wnd.Top = Position.Y;
            wnd.Left = Position.X;

            if (WasMinimized)
            {
                wnd.WindowState = WindowState.Minimized;
            }

            if (WasMaximized)
            {
                wnd.WindowState = WindowState.Maximized;
            }

            wnd.Scroll.ScrollToVerticalOffset(ScrollOffset);
        }

        public ScreensStateDto ToDto()
        {
            return new ScreensStateDto()
            {
                ScreenRelativePath = _novelWindow.Novel.FullPath,
                Position = new Point(_novelWindow.Left, _novelWindow.Top),
                WasMinimized = _novelWindow.WindowState == WindowState.Minimized,
                WasMaximized = _novelWindow.WindowState == WindowState.Maximized,
                ScrollOffset = _novelWindow.Scroll.VerticalOffset
            };
        }
    }
}