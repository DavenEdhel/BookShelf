using System;
using BookWiki.Core;
using BookWiki.Core.Files.FileSystemModels;
using BookWiki.Presentation.Apple.Extentions;
using BookWiki.Presentation.Apple.Views.Common;
using CoreGraphics;
using UIKit;

namespace BookWiki.Presentation.Apple.Views.Main
{
    public class TabView : Control, IComparable, ICollectionBindable, ISelectable, IPageData
    {
        private readonly string _title;
        private UILabel _label;
        private int _margin;
        private int _marginLeft = 15;
        private CollectionView _collectionView;
        private CollectionItem _collectionItem;
        private UIButton _close;
        private bool _isDeletable = true;

        public object Data { get; }

        public Action<object> OnSelected { private get; set; }

        public int Index { get; }

        public TabView(IContent article, int index)
        {
            Data = article;
            Index = index;
            _title = article.Title;
            Initialize();
        }

        public TabView(IQuery query, int index)
        {
            _title = query.Title;
            Data = query;
            Index = index;
            Initialize();
        }

        public TabView(IFileSystemNode root)
        {
            _title = "Файлы";
            Data = root;
            Initialize();
        }

        public bool IsSelected
        {
            get => BackgroundColor == UIColor.LightGray;
            set
            {
                var old = IsSelected;

                BackgroundColor = value ? UIColor.LightGray : UIColor.Clear;

                if (value && value != old)
                {
                    OnSelected?.Invoke(Data);
                }
            }
        }

        public bool IsDeletable
        {
            get => _isDeletable;
            set
            {
                _isDeletable = value;

                _close.Hidden = !IsDeletable;
            }
        }

        public bool IsDefaultTab { get; set; } = true;
        

        private void Initialize()
        {
            ClipsToBounds = true;

            _label = new UILabel();
            _label.Lines = 0;
            _label.Text = _title;
            Add(_label);

            _close = new UIButton(UIButtonType.Custom);
            _close.SetImage(UIImage.FromBundle("close.png"), UIControlState.Normal);
            _close.TouchUpInside += CloseOnTouchUpInside;
            _close.Hidden = !IsDeletable;
            Add(_close);

            Layout = () =>
            {
                _margin = 10;

                _label.SetSizeThatFits(Frame.Width - _margin - _marginLeft - 35);
                _label.ChangeY(_margin);
                _label.ChangeX(_marginLeft);

                _close.SetSizeThatFits();
                _close.PositionToRightAndCenterInside(this, _margin);
            };

            Layout();

            TouchUpInside += OnTouchUpInside;
        }

        private void OnTouchUpInside(object sender, EventArgs e)
        {
            _collectionView?.Select(_collectionItem);
        }

        private void CloseOnTouchUpInside(object sender, EventArgs e)
        {
            _close.RemoveFromSuperview();
            _close.TouchUpInside -= CloseOnTouchUpInside;

            _collectionView?.Remove(_collectionItem);
        }

        public override CGSize SizeThatFits(CGSize size)
        {
            var x = _label.SizeThatFits(new CGSize(size.Width - _margin - _marginLeft - 35, nfloat.MaxValue));

            return new CGSize(size.Width, x.Height + _margin * 2);
        }

        public void BindWithCollection(CollectionView collectionView, CollectionItem collectionItem)
        {
            _collectionView = collectionView;
            _collectionItem = collectionItem;
        }

        public int CompareTo(object obj)
        {
            if (obj is TabView tabView)
            {
                if (tabView.Data is IFileSystemNode)
                {
                    return 1;
                }

                if (tabView.Data is IContent)
                {
                    if (Data is IContent)
                    {
                        return Index.CompareTo(tabView.Index);
                    }

                    return -1;
                }

                if (tabView.Data is IQuery)
                {
                    if (Data is IQuery)
                    {
                        return Index.CompareTo(tabView.Index);
                    }

                    return -1;
                }
            }

            return 0;
        }

        public bool EqualsTo(object anotherData)
        {
            return Data.Equals(anotherData);
        }
    }
}