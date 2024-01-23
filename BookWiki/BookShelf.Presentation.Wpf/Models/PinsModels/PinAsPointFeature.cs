using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BookMap.Presentation.Apple.Services;
using BookMap.Presentation.Wpf;
using BookMap.Presentation.Wpf.Models;
using BookShelf.Presentation.Wpf.Models.PinsModels;
using BookWiki.Core;
using Newtonsoft.Json;

namespace BookWiki.Presentation.Wpf.Models.PinsModels;

public class ArticlePinInfoFromPayload
{
    public ArticlePinInfoFromPayload(string payload)
    {
        if (string.IsNullOrWhiteSpace(payload))
        {
            Data = new ArticlePinInfo();
        }
        else
        {
            Data = JsonConvert.DeserializeObject<ArticlePinInfo>(payload);
        }
    }

    public ArticlePinInfo Data { get; }
}

public class ArticlePinPayloadFromInfo
{
    public ArticlePinPayloadFromInfo(ArticlePinInfo info)
    {
        Payload = JsonConvert.SerializeObject(info);
    }

    public string Payload { get; }
}

public static class ArticlePinApi
{
    public static ArticlePinInfoFromPayload ToPin(this string payload)
    {
        return new ArticlePinInfoFromPayload(payload);
    }
}

public class ArticlePinInfo
{
    public string ArticleId { get; set; } = string.Empty;
}

public class PinAsArticleLinkFeature : IPinsFeature
{
    public bool CanCreateNew => false;

    public PinInfo NewPin()
    {
        return null;
    }

    public UIElement CreateView(PinDto info)
    {
        var article = BooksApplication.Instance.Articles.TryGet(info.Payload.ToPin().Data.ArticleId);

        if (article != null)
        {
            return new LinkToArticleView(article);
        }

        return new Canvas();
    }

    public void SetNormal(UIElement view)
    {
    }

    public void SetHighlighted(UIElement view)
    {
    }
}

public class PinAsPointFeature : IPinsFeature
{
    private readonly Article _article;
    private readonly MapView _mapView;
    private readonly PinAsArticleLinkFeature _links = new();

    public PinAsPointFeature(Article article, MapView mapView)
    {
        _article = article;
        _mapView = mapView;
    }

    public bool CanCreateNew => true;

    public PinInfo NewPin()
    {
        var alreadyAdded = _mapView.Pins.FirstOrDefault(x => x.Data.Payload.ToPin().Data.ArticleId == _article.Id);

        if (alreadyAdded != null)
        {
            _mapView.Pins.Remove(alreadyAdded.Data.Id);
        }

        return new PinInfo()
        {
            Name = Guid.NewGuid().ToString(),
            Payload = new ArticlePinPayloadFromInfo(
                new ArticlePinInfo()
                {
                    ArticleId = _article.Id
                }
            ).Payload
        };
    }

    public UIElement CreateView(PinDto info)
    {
        if (info.Payload.ToPin().Data.ArticleId == _article.Id)
        {
            return new PinView();
        }

        return _links.CreateView(info);
    }

    public void SetNormal(UIElement view)
    {
        if (view is not PinView)
        {
            _links.SetNormal(view);
        }
    }

    public void SetHighlighted(UIElement view)
    {
        if (view is not PinView)
        {
            _links.SetHighlighted(view);
        }
    }

    public class PinView : Canvas
    {
        public PinView()
        {
            var img = new Image()
            {
                Stretch = Stretch.UniformToFill,
                Source = new BitmapImage(new Uri("pack://application:,,,/Resources/marker.png"))
            };

            img.Width = 30;
            img.Height = 40;

            Width = 30;
            Height = 40;

            Canvas.SetLeft(img, -15);
            Canvas.SetTop(img, -40);

            Children.Add(img);
        }
    }
}