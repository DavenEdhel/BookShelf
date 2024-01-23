using System;
using System.Reactive.Disposables;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using BookMap.Presentation.Apple.Services;
using BookMap.Presentation.Wpf.Models;
using BookShelf.Presentation.Wpf.Views;
using Keurig.IQ.Core.CrossCutting.Extensions;

namespace BookWiki.Presentation.Wpf.Views;

// todo:
//  ability to remove bookmark
public class BookmarksView : StackPanel
{
    private readonly CompositeDisposable _scope = new();
    private Bookmarks _bookmarks;

    public BookmarksView()
    {
        Background = Brushes.LightGray;
    }

    public void Start(Bookmarks bookmarks)
    {
        _bookmarks = bookmarks;

        bookmarks.Subscribe(x => Render()).InScopeOf(_scope);

        Render();
    }

    public void Stop()
    {
        _scope.Dispose();
    }

    private void Render()
    {
        foreach (Button button in Children)
        {
            button.Click -= ItemClicked;
        }

        Children.Clear();

        var add = new TabItemView("Запомнить Место");
        add.Click += AddOnClick;

        Children.Add(add);

        foreach (var bookmark in _bookmarks)
        {
            Render(bookmark);
        }
    }

    private void AddOnClick(object sender, RoutedEventArgs e)
    {
        var contentWindow = new NewBookmarkWindow();
        if (contentWindow.ShowDialog() == true)
        {
            _bookmarks.Make(contentWindow.ContentName.Text);
        }
    }

    private void Render(Bookmark bookmark)
    {
        var button = new TabItemView(bookmark.Name);
        button.Tag = bookmark;
        Children.Add(button);

        button.Click += ItemClicked;
    }

    private void ItemClicked(object sender, RoutedEventArgs e)
    {
        var b = sender.CastTo<Button>();

        var w = b.Tag.CastTo<Bookmark>();

        w.Apply();
    }

    //public class BookmarkItemView : Button
    //{
    //    private readonly Bookmark _b;

    //    public BookmarkItemView(Bookmark b)
    //    {
    //        _b = b;
    //        BorderThickness = new Thickness(0);
    //        Height = 30;
    //        Background = Brushes.White;
    //        HorizontalContentAlignment = HorizontalAlignment.Left;
    //        VerticalContentAlignment = VerticalAlignment.Center;
    //        Padding = new Thickness(10, 0, 0, 0);

    //        var fileNodeName = new TextBlock();
    //        fileNodeName.FontFamily = new FontFamily("Times New Roman");
    //        fileNodeName.FontSize = 14;
    //        fileNodeName.Text = b.Name;
    //        fileNodeName.TextAlignment = TextAlignment.Center;

    //        Content = fileNodeName;

    //        this.Click += OnClick;
    //    }

    //    private void OnClick(object sender, RoutedEventArgs e)
    //    {
    //        _b.Apply();
    //    }
    //}
}

// todo:
//  ability to remove pin