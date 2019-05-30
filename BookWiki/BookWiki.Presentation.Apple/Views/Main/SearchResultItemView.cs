using System;
using System.Linq;
using BookWiki.Core;
using BookWiki.Presentation.Apple.Extentions;
using BookWiki.Presentation.Apple.Views.Common;
using BookWiki.Presentation.Apple.Views.Controls;
using CoreGraphics;
using UIKit;

namespace BookWiki.Presentation.Apple.Views.Main
{
    public class SearchResultItemView : Control, ISelectable, IPageData, ICollectionBindable
    {
        private readonly SearchResult _searchResult;
        private CollectionView _collectionView;
        private CollectionItem _collectionItem;
        private bool _isSelected;

        public Action<IContent> OnSelected = delegate {  };

        private UILabel _title;

        public SearchResultItemView(SearchResult searchResult)
        {
            _searchResult = searchResult;

            Initialize();
        }

        private void Initialize()
        {
            UserInteractionEnabled = true;
            ClipsToBounds = true;

            Layer.BorderWidth = 1;
            Layer.BorderColor = UIColor.LightGray.CGColor;

            _title = new UILabel();
            _title.Font = UIFont.BoldSystemFontOfSize(20);
            _title.Text = _searchResult.Article.Title;
            Add(_title);

            foreach (var searchResultFinding in _searchResult.Findings.Take(5).ToArray())
            {
                var part = new ArticlePartView(searchResultFinding.Normalize());
                Add(part);
            }

            Layout = () =>
            {
                _title.SetSizeThatFits(Frame.Width);
                _title.ChangePosition(0, 0);

                var y = _title.Frame.Bottom;

                foreach (var subview in Subviews.ToArray().Where(x => x is ArticlePartView).Cast<ArticlePartView>())
                {
                    subview.SetSizeThatFits(Frame.Width);
                    subview.ChangeX(0);
                    subview.ChangeY(y);

                    y = subview.Frame.Bottom + 5;
                }
            };

            Layout();

            TouchUpInside += OnTouchUpInside;
        }

        private void OnTouchUpInside(object sender, EventArgs e)
        {
            _collectionView.Select(_collectionItem);
        }

        public override CGSize SizeThatFits(CGSize size)
        {
            Layout();

            return new CGSize(size.Width - 20, Subviews.Last().Frame.Bottom + 5);
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                Layer.BorderColor = value ? UIColor.Black.CGColor : UIColor.LightGray.CGColor;
                Layer.BorderWidth = value ? 2 : 1;

                if (value)
                {
                    _isSelected = value;

                    OnSelected(_searchResult.Article);
                }
            }
        }

        public bool EqualsTo(object anotherData)
        {
            return _searchResult.Article.Equals(anotherData);
        }

        public void BindWithCollection(CollectionView collectionView, CollectionItem collectionItem)
        {
            _collectionView = collectionView;
            _collectionItem = collectionItem;
        }
    }
}