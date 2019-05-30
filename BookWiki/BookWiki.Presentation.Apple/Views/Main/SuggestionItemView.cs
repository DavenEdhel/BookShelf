using System;
using System.Linq;
using BookWiki.Presentation.Apple.Extentions;
using BookWiki.Presentation.Apple.Views.Common;
using CoreGraphics;
using UIKit;

namespace BookWiki.Presentation.Apple.Views.Main
{
    public class SuggestionItemView : Control, ISelectable, IPageData, ICollectionBindable
    {
        private readonly string _suggestion;
        private CollectionView _collectionView;
        private CollectionItem _collectionItem;
        private bool _isSelected;

        public Action<string> OnSelected = delegate { };

        private UILabel _title;

        public SuggestionItemView(string suggestion)
        {
            _suggestion = suggestion;

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
            _title.Text = _suggestion;
            Add(_title);

            Layout = () =>
            {
                _title.SetSizeThatFits(Frame.Width);
                _title.ChangePosition(0, 0);
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
                BackgroundColor = value ? UIColor.LightGray : UIColor.White;

                _isSelected = value;

                if (value)
                {
                    OnSelected(_suggestion);
                }
            }
        }

        public bool EqualsTo(object anotherData)
        {
            return _suggestion.Equals(anotherData);
        }

        public void BindWithCollection(CollectionView collectionView, CollectionItem collectionItem)
        {
            _collectionView = collectionView;
            _collectionItem = collectionItem;
        }
    }
}