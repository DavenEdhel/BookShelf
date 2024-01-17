using System.Windows;

namespace BookWiki.Presentation.Wpf.Extensions
{
    public static class WindowExtensions
    {
        public static void ActivateOrRestore(this Window wnd, bool fullscreen = false)
        {
            if (fullscreen)
            {
                wnd.WindowState = WindowState.Maximized;
            }

            if (wnd.WindowState == WindowState.Minimized)
            {
                wnd.WindowState = WindowState.Normal;
            }

            wnd.Activate();
        }
    }
}