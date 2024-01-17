using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BookWiki.Presentation.Wpf.Views
{
    public class TabItemView : Button
    {
        public TabItemView(string title)
        {
            BorderThickness = new Thickness(0);
            Height = 30;
            Background = Brushes.White;
            HorizontalContentAlignment = HorizontalAlignment.Left;
            VerticalContentAlignment = VerticalAlignment.Center;
            Padding = new Thickness(10, 0, 0, 0);

            var fileNodeName = new TextBlock();
            fileNodeName.FontFamily = new FontFamily("Times New Roman");
            fileNodeName.FontSize = 14;
            fileNodeName.Text = title;
            fileNodeName.TextAlignment = TextAlignment.Center;

            Content = fileNodeName;
        }
    }
}