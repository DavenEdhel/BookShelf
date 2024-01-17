using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BookWiki.Presentation.Wpf.Views
{
    public class ClickablePanel : Button
    {
        public ClickablePanel()
        {
            Background = new SolidColorBrush(Colors.Transparent);
            BorderThickness = new Thickness(0);
        }
    }
}