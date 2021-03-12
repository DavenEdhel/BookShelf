using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using BookWiki.Core.Files.FileSystemModels;
using BookWiki.Core.Files.PathModels;

namespace BookWiki.Presentation.Wpf.Views
{
    public class FileSystemView : StackPanel
    {
        private IFileSystemNode _node;
        private readonly TextBlock _expandButtonText;
        private StackPanel _childItems;
        private int _offset = 20;

        public FileSystemView(IFileSystemNode node)
        {
            _node = node;
            Orientation = Orientation.Vertical;

            var itemStack = new StackPanel();
            itemStack.VerticalAlignment = VerticalAlignment.Center;
            itemStack.Orientation = Orientation.Horizontal;
            itemStack.Margin = new Thickness(_offset, 0, 0, 0);

            var expandButton = new Button();
            expandButton.Visibility = node.IsContentFolder ? Visibility.Hidden : Visibility.Visible;
            expandButton.Width = 30;
            expandButton.Height = 30;
            expandButton.Click += ExpandButtonOnClick;
            expandButton.Background = Brushes.White;

            Grid.SetColumn(expandButton, 1);

            _expandButtonText = new TextBlock();
            _expandButtonText.Text = "+";
            _expandButtonText.FontFamily = new FontFamily("Times New Roman");
            _expandButtonText.FontSize = 16;
            _expandButtonText.TextAlignment = TextAlignment.Center;
            expandButton.Content = _expandButtonText;

            itemStack.Children.Add(expandButton);

            var fileNodeName = new TextBlock();
            fileNodeName.MouseUp += FileNodeNameOnMouseUp;
            fileNodeName.Height = 30;
            fileNodeName.Margin = new Thickness(10, 10, 0, 0);
            fileNodeName.FontFamily = new FontFamily("Times New Roman");
            fileNodeName.FontSize = 16;
            fileNodeName.Text = node.Path.Name.PlainText;
            fileNodeName.TextAlignment = TextAlignment.Center;

            itemStack.Children.Add(fileNodeName);

            Children.Add(itemStack);

            _childItems = new StackPanel();
            _childItems.Margin = new Thickness(_offset*2, 0, 0, 0);
            _childItems.Orientation = Orientation.Vertical;
            Children.Add(_childItems);

            if (_node.IsContentFolder == false)
            {
                var shouldBeOpened = BookShelf.Instance.Session.OpenedContentTabs.Any(x => x.NovelPathToLoad.Contains(_node.Path.RelativePath(BookShelf.Instance.RootPath)));

                if (shouldBeOpened)
                {
                    Expand();
                }
            }
        }

        private void FileNodeNameOnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_node.IsContentFolder == false)
            {
                var contentWindow = new NewContentWindow();
                if (contentWindow.ShowDialog() == true)
                {
                    BookShelf.Instance.Save(_node, new FileName(contentWindow.ContentName.Text), contentWindow.Extension);

                    _node = new FileSystemNode(_node.Path.FullPath);

                    Collapse();
                    Expand();
                }
            }
            else
            {
                BookShelf.Instance.Open(_node.Path.RelativePath(BookShelf.Instance.RootPath));
            }
        }

        private void ExpandButtonOnClick(object sender, RoutedEventArgs e)
        {
            if (_expandButtonText.Text == "+")
            {
                _expandButtonText.Text = "-";

                Expand();
            }
            else
            {
                _expandButtonText.Text = "+";

                Collapse();
            }
        }

        private void Expand()
        {
            foreach (var fileSystemNode in _node.InnerNodes)
            {
                _childItems.Children.Add(new FileSystemView(fileSystemNode));
            }
        }

        private void Collapse()
        {
            _childItems.Children.Clear();
        }
    }
}