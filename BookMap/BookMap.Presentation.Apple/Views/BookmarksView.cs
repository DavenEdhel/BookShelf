using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookMap.Presentation.Apple.Services;
using CoreGraphics;
using Foundation;
using UIKit;

namespace BookMap.Presentation.Apple.Views
{
    public class BookmarksView : UIView
    {
        private readonly MapView _mapView;
        private readonly MapProvider _mapProvider;
        private UITableView _list;
        private BookmarkTableViewSource _source;
        private UIButton _new;
        private AddBookmarkView _addBookmarkMapView;
        private UIButton _remove;
        private UIButton _update;

        class BookmarkTableViewSource : UITableViewSource
        {
            private readonly MapProvider _mapProvider;

            private IEnumerable<Bookmark> _items;

            public event Action<Bookmark> BookmarkSelected = delegate { };

            public BookmarkTableViewSource(MapProvider mapProvider)
            {
                _mapProvider = mapProvider;
                _items = _mapProvider.Settings.Bookmarks;
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                var cell = tableView.DequeueReusableCell("cell");

                if (cell == null)
                {
                    cell = new UITableViewCell();
                }

                cell.TextLabel.Text = _items.ElementAt(indexPath.Row).Name;

                return cell;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                BookmarkSelected(_items.ElementAt(indexPath.Row));
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                return _items.Count();
            }

            public int GetSelectedIndex(UITableView list)
            {
                return list.IndexPathForSelectedRow?.Row ?? -1;
            }

            public Bookmark GetSelectedBookmark(UITableView list)
            {
                return _items.ElementAtOrDefault(GetSelectedIndex(list));
            }

            public void Select(string bookmark, UITableView list)
            {
                var i = IndexOf(_items, x => string.Equals(x.Name, bookmark, StringComparison.InvariantCultureIgnoreCase));

                if (i > 0)
                {
                    list.SelectRow(NSIndexPath.FromIndex((nuint)i), false, UITableViewScrollPosition.None);
                }
            }

            private int IndexOf<T>(IEnumerable<T> items, Func<T, bool> predecate)
            {
                for (int i = 0; i < items.Count(); i++)
                {
                    if (predecate(items.ElementAt(i)))
                    {
                        return i;
                    }
                }

                return -1;
            }
        }

        public BookmarksView(MapView mapView, MapProvider mapProvider)
        {
            _mapView = mapView;
            _mapProvider = mapProvider;
            Initialize();
        }

        public Bookmark SelectedBookmark => _source.GetSelectedBookmark(_list);

        public void Select(string bookmark)
        {
            _source.Select(bookmark, _list);
        }

        private void Initialize()
        {
            BackgroundColor = UIColor.Gray.ColorWithAlpha(0.5f);

            _mapProvider.MapChanged += MapProviderOnSettingsChanged;

            _list = new UITableView();
            _list.BackgroundColor = UIColor.LightGray.ColorWithAlpha(0.5f);
            _list.Source = _source = new BookmarkTableViewSource(_mapProvider);
            _source.BookmarkSelected += SourceOnBookmarkSelected;
            _list.AllowsMultipleSelection = false;
            Add(_list);

            _addBookmarkMapView = new AddBookmarkView(_mapView);
            _addBookmarkMapView.Created += BookmarkCreated;
            _addBookmarkMapView.Canceled += AddBookmarkMapViewOnCanceled;

            _new = new UIButton(UIButtonType.RoundedRect);
            _new.SetTitle("Add", UIControlState.Normal);
            _new.SetTitleColor(UIColor.Blue, UIControlState.Normal);
            _new.TouchUpInside += NewOnTouchUpInside;
            Add(_new);

            _update = new UIButton(UIButtonType.RoundedRect);
            _update.SetTitle("Update", UIControlState.Normal);
            _update.SetTitleColor(UIColor.Blue, UIControlState.Normal);
            _update.TouchUpInside += UpdateOnTouchUpInside;
            Add(_update);

            _remove = new UIButton(UIButtonType.RoundedRect);
            _remove.SetTitle("Remove", UIControlState.Normal);
            _remove.SetTitleColor(UIColor.Red, UIControlState.Normal);
            _remove.TouchUpInside += RemoveOnTouchUpInside;
            Add(_remove);
        }

        private void UpdateOnTouchUpInside(object sender, EventArgs e)
        {
            var index = _source.GetSelectedIndex(_list);

            _mapProvider.UpdateBookmark(_mapView.ExtractBookmark().World, index);

            ReloadBookmarks();

            _list.SelectRow(NSIndexPath.FromRowSection(index, 0), false, UITableViewScrollPosition.None);
        }

        private void MapProviderOnSettingsChanged(string obj)
        {
            ReloadBookmarks();
        }

        private void AddBookmarkMapViewOnCanceled()
        {
            _addBookmarkMapView.RemoveFromSuperview();
        }

        private void BookmarkCreated(Bookmark s)
        {
            _mapProvider.AddBookmark(s);

            ReloadBookmarks();

            _addBookmarkMapView.RemoveFromSuperview();
        }

        private void ReloadBookmarks()
        {
            if (_source != null)
            {
                _source.BookmarkSelected -= SourceOnBookmarkSelected;
            }

            _source = new BookmarkTableViewSource(_mapProvider);
            _source.BookmarkSelected += SourceOnBookmarkSelected;
            _list.Source = _source;
            _list.ReloadData();
        }

        private void SourceOnBookmarkSelected(Bookmark bookmark)
        {
            _mapView.PositionMapToBookmark(bookmark);
        }

        private void NewOnTouchUpInside(object sender, EventArgs e)
        {
            Superview.Add(_addBookmarkMapView);
        }

        private void RemoveOnTouchUpInside(object sender, EventArgs e)
        {
            var index = _source.GetSelectedIndex(_list);

            _mapProvider.RemoveBookmark(index);

            ReloadBookmarks();
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            _list.Frame = new CGRect(new CGPoint(0, 100), new CGSize(Frame.Width, Frame.Height - 150)); ;
            _addBookmarkMapView.Frame = new CGRect(Window.Frame.Width / 3, Window.Frame.Height / 3, Window.Frame.Width / 3, Window.Frame.Height / 3);
            _new.Frame = new CGRect(new CGPoint(0, 0), new CGSize(Frame.Width, 50));
            _update.Frame = new CGRect(new CGPoint(0, 50), new CGSize(Frame.Width, 50));
            _remove.Frame = new CGRect(new CGPoint(0, Frame.Height - 50), new CGSize(Frame.Width, 50));
        }
    }
}