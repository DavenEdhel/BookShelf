using System;
using System.Collections.Generic;
using System.Linq;
using BookWiki.Presentation.Apple.Extentions;
using CoreGraphics;
using UIKit;

namespace BookWiki.Presentation.Apple.Views.Common
{
    public class CollectionView : View
    {
        private UIScrollView _scroll;

        private readonly List<CollectionItem> _collectionItems = new List<CollectionItem>();

        public event Action ScrolledToBottom = delegate {  };

        public bool IsOrderingEnabled { get; set; } = true;

        public event Action Selected = delegate { };
        public event Action Selecting = delegate { };

        public CollectionView()
        {
            Initialize();
        }

        public CollectionItem this[int index]
        {
            get
            {
                if (index >= _collectionItems.Count)
                {
                    return null;
                }

                return _collectionItems[index];
            }
        }

        public CollectionItem[] Items => _collectionItems.ToArray();

        private void Initialize()
        {
            _scroll = new UIScrollView();
            _scroll.Scrolled += ScrollOnScrolled;
            Add(_scroll);

            Layout = () =>
            {
                _scroll.Frame = new CGRect(0, 0, Frame.Width, Frame.Height);

                nfloat bottom = 0;
                foreach (var item in _collectionItems)
                {
                    item.Content.SetSizeThatFits(width: Frame.Width);
                    item.Content.ChangeX(0);
                    item.Content.ChangeY(bottom);

                    bottom += item.Content.Frame.Height;

                    item.Separator.SetSizeThatFits(Frame.Width);
                    item.Separator.ChangeX(0);
                    item.Separator.ChangeY(bottom);

                    bottom += item.Separator.Frame.Height;
                }

                _scroll.ContentSize = new CGSize(Frame.Width, bottom);
            };

            Layout();
        }

        public CGSize ChangeWidthAndLayout(float width)
        {
            this.ChangeWidth(width);

            Layout();

            this.ChangeHeight(_scroll.ContentSize.Height);

            Layout();

            return new CGSize(width, _scroll.ContentSize.Height);
        }

        private void ScrollOnScrolled(object sender, EventArgs e)
        {
            var scrollViewHeight = _scroll.Frame.Size.Height;
            var scrollContentSizeHeight = _scroll.ContentSize.Height;
            var scrollOffset = _scroll.ContentOffset.Y;

            if (scrollOffset == 0)
            {
                // then we are at the top
            }
            else if (scrollOffset + scrollViewHeight + 100 > scrollContentSizeHeight)
            {
                ScrolledToBottom();
            }
        }

        public void Remove(CollectionItem item, bool animated = true)
        {
            if (item.Content is ISelectable selectable)
            {
                if (selectable.IsSelected)
                {
                    Select(_collectionItems.First());
                }
            }

            if (animated)
            {
                Animate(0.4,
                    () =>
                    {
                        item.Content.ChangeHeight(0);
                        item.Separator.Alpha = 0;
                        item.Separator.ChangeHeight(1);
                        item.Separator.ChangePosition(0, item.Content.Frame.Top);

                        _collectionItems.Remove(item);

                        Layout();
                    },
                    () =>
                    {
                        item.Content.RemoveFromSuperview();
                        item.Separator.RemoveFromSuperview();
                    });
            }
            else
            {
                _collectionItems.Remove(item);
                
                item.Content.RemoveFromSuperview();
                item.Separator.RemoveFromSuperview();

                Layout();
            }
        }

        public void Add(CollectionItem item, bool animated = true)
        {
            _collectionItems.Add(item);

            if (IsOrderingEnabled)
            {
                _collectionItems.Sort();
            }

            var index = _collectionItems.IndexOf(item);

            if (index == 0)
            {
                item.Content.Frame = new CGRect(0, 0, Frame.Width, 0);
                item.Separator.Frame = new CGRect(0, 0, Frame.Width, 0);
            }
            else
            {
                item.Content.Frame = new CGRect(0, _collectionItems[index - 1].Separator.Frame.Bottom, Frame.Width, 0);
                item.Separator.Frame = new CGRect(0, _collectionItems[index - 1].Separator.Frame.Bottom, Frame.Width, 0);
            }

            AddViews(item);

            if (animated)
            {
                Animate(0.4,
                    () =>
                    {
                        Layout();
                    });
            }
            else
            {
                Layout();
            }
            
        }

        public void AddRangeWithoutAnimation(params CollectionItem[] items)
        {
            _collectionItems.AddRange(items);

            if (IsOrderingEnabled)
            {
                _collectionItems.Sort();
            }
            

            foreach (var collectionItem in items)
            {
                AddViews(collectionItem);
            }

            Layout();
        }

        public void Select(CollectionItem item)
        {
            if (item == null)
            {
                return;
            }

            Selecting();

            foreach (var collectionItem in _collectionItems)
            {
                if (collectionItem.Content is ISelectable selectable)
                {
                    selectable.IsSelected = false;
                }
            }

            if (item.Content is ISelectable selectableItem)
            {
                selectableItem.IsSelected = true;
            }

            Selected();
        }

        private void AddViews(CollectionItem collectionItem)
        {
            _scroll.Add(collectionItem.Content);
            _scroll.Add(collectionItem.Separator);

            if (collectionItem.Content is ICollectionBindable bindable)
            {
                bindable.BindWithCollection(this, collectionItem);
            }
        }
    }
}