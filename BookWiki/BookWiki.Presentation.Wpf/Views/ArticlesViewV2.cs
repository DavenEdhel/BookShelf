using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BookWiki.Core.Articles;
using Keurig.IQ.Core.CrossCutting.Extensions;

namespace BookWiki.Presentation.Wpf.Views
{
    public class ArticlesViewV2 : ScrollViewer
    {
        private readonly StackPanel _stack;

        public ArticlesViewV2()
        {
            this.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;

            _stack = new StackPanel();
            _stack.Orientation = Orientation.Vertical;

            this.Content = _stack;
        }

        public void Set(IEnumerable<ArticleSearchResult> articles)
        {
            var count = Math.Max(_stack.Children.Count, articles.Count());

            for (int i = 0; i < count; i++)
            {
                if (i >= _stack.Children.Count)
                {
                    // create new item
                    _stack.Children.Add(new ArticleItemView());
                }

                if (i >= articles.Count())
                {
                    // not need to display
                    _stack.Children[i].CastTo<ArticleItemView>().Visibility = Visibility.Collapsed;

                    continue;
                }

                var view = _stack.Children[i].CastTo<ArticleItemView>();
                view.Visibility = Visibility.Visible;
                view.Set(articles.ElementAt(i));
            }

            _stack.Children.Clear();
            foreach (var article in articles)
            {
                _stack.Children.Add(new ArticleItemView(article));
            }
        }

        public IEnumerable<ArticleItemView> All
        {
            get
            {
                foreach (ArticleItemView articleItemView in _stack.Children)
                {
                    if (articleItemView.Visibility == Visibility.Visible)
                    {
                        yield return articleItemView;
                    }
                }
            }
        }

        public bool AtLeastOneSelected => All.Any(x => x.IsSelected);
        public bool OnlyOneExist => All.Count() == 1;
        public bool NothingFound => All.Count() == 0;
        public bool NothingIsSelected => All.Any(x => x.IsSelected) == false;

        public void Down()
        {
            if (All.Any() == false)
            {
                return;
            }

            if (All.All(x => x.IsSelected == false))
            {
                All.First().IsSelected = true;
                return;
            }

            bool selectNext = false;
            ArticleItemView prev = null;
            foreach (ArticleItemView articleItemView in All)
            {
                if (selectNext)
                {
                    articleItemView.IsSelected = true;
                    prev.IsSelected = false;
                    return;
                }

                if (articleItemView.IsSelected)
                {
                    selectNext = true;
                    prev = articleItemView;
                }
            }
        }

        public void Up()
        {
            if (All.Any() == false)
            {
                return;
            }

            if (All.All(x => x.IsSelected == false))
            {
                All.Last().IsSelected = true;
                return;
            }

            ArticleItemView prev = null;
            foreach (ArticleItemView articleItemView in All)
            {
                if (articleItemView.IsSelected)
                {
                    if (prev != null)
                    {
                        prev.IsSelected = true;
                        articleItemView.IsSelected = false;
                    }

                    return;
                }

                prev = articleItemView;
            }
        }

        public void NavigateToSelected()
        {
            ArticleItemView prev = null;
            foreach (ArticleItemView articleItemView in All)
            {
                if (articleItemView.IsSelected)
                {
                    prev = articleItemView;
                    break;
                }
            }

            if (prev != null)
            {
                prev.Open();
            }
        }

        public void NavigateToFirst()
        {
            foreach (ArticleItemView articleItemView in All)
            {
                articleItemView.Open();
                return;
            }
        }
    }
}