using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using BookWiki.Core.Files.FileSystemModels;
using BookWiki.Core.Files.PathModels;

namespace BookWiki.Presentation.Wpf.Views;

public class ArticleNodeView : StackPanel
{
    private IFileSystemNode _node;
    private StackPanel _childItems;
    private int _offset = 20;

    public ArticleNodeView(IFileSystemNode node)
    {
        _node = node;
        Orientation = Orientation.Vertical;

        var itemGrid = new Grid();
        itemGrid.ColumnDefinitions.Add(new ColumnDefinition()
        {
            Width = GridLength.Auto
        });
        itemGrid.ColumnDefinitions.Add(new ColumnDefinition());

        var itemStack = new StackPanel();
        Grid.SetColumn(itemStack, 0);
        itemStack.VerticalAlignment = VerticalAlignment.Center;
        itemStack.Orientation = Orientation.Horizontal;
        itemStack.Margin = new Thickness(_offset, 0, 0, 0);

        var fileNodeName = new TextBlock();
        fileNodeName.MouseUp += FileNodeNameOnMouseUp;
        fileNodeName.Height = 30;
        fileNodeName.Margin = new Thickness(10, 10, 0, 0);
        fileNodeName.FontFamily = new FontFamily("Times New Roman");
        fileNodeName.FontSize = 16;
        fileNodeName.Text = node.Path.Name.PlainText;
        fileNodeName.TextAlignment = TextAlignment.Center;

        itemStack.Children.Add(fileNodeName);

        var buttons = new StackPanel();
        Grid.SetColumn(buttons, 1);
        buttons.VerticalAlignment = VerticalAlignment.Center;
        buttons.Orientation = Orientation.Horizontal;
        buttons.HorizontalAlignment = HorizontalAlignment.Right;
        buttons.Margin = new Thickness(_offset, 0, 0, 0);

        Children.Add(itemGrid);

        itemGrid.Children.Add(itemStack);
        itemGrid.Children.Add(buttons);

        _childItems = new StackPanel();
        _childItems.Margin = new Thickness(_offset * 2, 0, 0, 0);
        _childItems.Orientation = Orientation.Vertical;
        Children.Add(_childItems);
    }

    private void FileNodeNameOnMouseUp(object sender, MouseButtonEventArgs e)
    {
        BooksApplication.Instance.OpenArticle(_node.Path.RelativePath(BooksApplication.Instance.RootPath));

        BooksApplication.Instance.AllArticlesWindow?.Close();
    }
}

public class MapNodeView : StackPanel
{
    private IFileSystemNode _node;
    private StackPanel _childItems;
    private int _offset = 20;

    public MapNodeView(IFileSystemNode node)
    {
        _node = node;
        Orientation = Orientation.Vertical;

        var itemGrid = new Grid();
        itemGrid.ColumnDefinitions.Add(new ColumnDefinition()
        {
            Width = GridLength.Auto
        });
        itemGrid.ColumnDefinitions.Add(new ColumnDefinition());

        var itemStack = new StackPanel();
        Grid.SetColumn(itemStack, 0);
        itemStack.VerticalAlignment = VerticalAlignment.Center;
        itemStack.Orientation = Orientation.Horizontal;
        itemStack.Margin = new Thickness(_offset, 0, 0, 0);

        var fileNodeName = new TextBlock();
        fileNodeName.MouseUp += FileNodeNameOnMouseUp;
        fileNodeName.Height = 30;
        fileNodeName.Margin = new Thickness(10, 10, 0, 0);
        fileNodeName.FontFamily = new FontFamily("Times New Roman");
        fileNodeName.FontSize = 16;
        fileNodeName.Text = node.Path.Name.PlainText;
        fileNodeName.TextAlignment = TextAlignment.Center;

        itemStack.Children.Add(fileNodeName);

        var buttons = new StackPanel();
        Grid.SetColumn(buttons, 1);
        buttons.VerticalAlignment = VerticalAlignment.Center;
        buttons.Orientation = Orientation.Horizontal;
        buttons.HorizontalAlignment = HorizontalAlignment.Right;
        buttons.Margin = new Thickness(_offset, 0, 0, 0);

        Children.Add(itemGrid);

        itemGrid.Children.Add(itemStack);
        itemGrid.Children.Add(buttons);

        _childItems = new StackPanel();
        _childItems.Margin = new Thickness(_offset * 2, 0, 0, 0);
        _childItems.Orientation = Orientation.Vertical;
        Children.Add(_childItems);
    }

    private void FileNodeNameOnMouseUp(object sender, MouseButtonEventArgs e)
    {
        BooksApplication.Instance.OpenMap(_node.Path.RelativePath(BooksApplication.Instance.RootPath), fullscreen: true);
    }
}