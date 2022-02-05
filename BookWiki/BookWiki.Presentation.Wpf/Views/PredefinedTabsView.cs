using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using BookWiki.Core.Files.PathModels;
using BookWiki.Presentation.Wpf.Extensions;
using BookWiki.Presentation.Wpf.Models;
using Keurig.IQ.Core.CrossCutting.Extensions;

namespace BookWiki.Presentation.Wpf.Views
{
    public class PredefinedTabsView : StackPanel
    {
        public PredefinedTabsView()
        {
            Background = Brushes.LightGray;
        }

        public void Start()
        {
            Render();
        }

        public void Stop()
        {
        }

        private void Render()
        {
            foreach (Button button in Children)
            {
                button.Click -= ItemClicked;
            }

            Children.Clear();

            foreach (var tab in BookShelf.Instance.Tabs.OrderBy(x => x.Title))
            {
                Render(tab);
            }
        }

        private void ItemClicked(object sender, RoutedEventArgs e)
        {
            var b = sender.CastTo<Button>();

            var w = b.Tag.CastTo<TabDto>();

            BookShelf.Instance.Open(new FolderPath(w.Path), fullscreen: true);
        }

        private void Render(TabDto tab)
        {
            var button = new TabItemView(tab.Title);
            button.Tag = tab;
            Children.Add(button);

            button.Click += ItemClicked;
        }

        public void ToggleVisibility(bool? toVisible = null)
        {
            if (toVisible == null)
            {
                if (Visibility == Visibility.Visible)
                {
                    Visibility = Visibility.Collapsed;
                }
                else
                {
                    Visibility = Visibility.Visible;
                }
            }
            else
            {
                Visibility = toVisible.Value ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }
}