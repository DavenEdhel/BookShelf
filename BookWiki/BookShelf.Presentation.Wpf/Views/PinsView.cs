using System;
using System.Reactive.Disposables;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using BookMap.Presentation.Wpf;
using BookMap.Presentation.Wpf.Models;
using BookWiki.Presentation.Wpf.Models.PinsModels;
using Keurig.IQ.Core.CrossCutting.Extensions;

namespace BookWiki.Presentation.Wpf.Views;

public class PinsView : StackPanel
{
    private readonly CompositeDisposable _scope = new();
    private IPins _pins;
    private MapView _map;

    public PinsView()
    {
        Background = Brushes.LightGray;
    }

    public void Start(IPins pins, MapView mapView)
    {
        _map = mapView;
        _pins = pins;

        pins.Changed.Subscribe(x => Render()).InScopeOf(_scope);

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

        foreach (var pin in _pins)
        {
            Render(pin);
        }
    }

    private void AddOnClick(object sender, RoutedEventArgs e)
    {
        //var contentWindow = new NewBookmarkWindow();
        //if (contentWindow.ShowDialog() == true)
        //{
        //    _bookmarks.Make(contentWindow.ContentName.Text);
        //}
    }

    private void Render(IPin bookmark)
    {
        var article = BooksApplication.Instance.Articles.TryGet(bookmark.Data.Payload.ToPin().Data.ArticleId);

        var button = new TabItemView(article?.Name ?? bookmark.Data.Name);
        button.Tag = bookmark;
        Children.Add(button);

        button.Click += ItemClicked;
    }

    private void ItemClicked(object sender, RoutedEventArgs e)
    {
        var b = sender.CastTo<Button>();

        var w = b.Tag.CastTo<IPin>();

        w.Highlight();

        var article = BooksApplication.Instance.Articles.TryGet(w.Data.Payload.ToPin().Data.ArticleId);

        if (article != null && article.MapLink.Region != null)
        {
            _map.Bookmarks.Restore(article.MapLink.Region);
        }
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