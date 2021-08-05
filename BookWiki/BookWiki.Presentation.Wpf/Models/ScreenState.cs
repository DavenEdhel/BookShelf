using System.Windows;
using BookWiki.Core.Files.PathModels;

namespace BookWiki.Presentation.Wpf.Models
{
    public class ScreenState
    {
        private readonly NovelWindow _novelWindow;
        public IRelativePath Novel { get; set; }

        public bool WasMinimized { get; set; }

        public Point Position { get; set; }

        public ScreenState(ScreensStateDto dto)
        {
            Novel = new FolderPath(dto.ScreenRelativePath);
            Position = dto.Position;
            WasMinimized = dto.WasMinimized;
        }

        public ScreenState(NovelWindow novelWindow)
        {
            _novelWindow = novelWindow;
        }

        public ScreensStateDto ToDto()
        {
            return new ScreensStateDto()
            {
                ScreenRelativePath = _novelWindow.Novel.FullPath,
                Position = new Point(_novelWindow.Left, _novelWindow.Top),
                WasMinimized = _novelWindow.WindowState == WindowState.Minimized
            };
        }
    }
}