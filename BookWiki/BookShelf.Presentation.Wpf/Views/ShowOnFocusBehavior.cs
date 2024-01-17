using System.Windows;
using System.Windows.Controls;

namespace BookWiki.Presentation.Wpf.Views
{
    public class ShowOnFocusBehavior
    {
        private readonly FrameworkElement _canBeCollapsed;

        public ShowOnFocusBehavior(TextBox focusable, FrameworkElement canBeCollapsed)
        {
            _canBeCollapsed = canBeCollapsed;
            focusable.GotFocus += QueryBoxOnGotFocus;
            focusable.LostFocus += QueryBoxOnLostFocus;
            canBeCollapsed.Visibility = Visibility.Hidden;
        }

        private void QueryBoxOnLostFocus(object sender, RoutedEventArgs e)
        {
            _canBeCollapsed.Visibility = Visibility.Collapsed;
        }

        private void QueryBoxOnGotFocus(object sender, RoutedEventArgs e)
        {
            _canBeCollapsed.Visibility = Visibility.Visible;
        }
    }
}