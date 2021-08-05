using System;
using System.Collections.Generic;
using System.Linq;
using BookMap.Presentation.Apple.Services;
using CoreGraphics;
using Foundation;
using UIKit;

namespace BookMap.Presentation.Apple.Views
{
    public class MenuView : UIView
    {
        private readonly MapView _mapView;
        private UITableView _list;
        private UIButton _load;
        private MenuTableViewSource _source;
        private UIButton _new;
        private AddNewMapView _addNewMapView;

        class MenuTableViewSource : UITableViewSource
        {
            private IEnumerable<string> _items;

            public MenuTableViewSource()
            {
                _items = FileSystemService.GetMapNames();
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                var cell = tableView.DequeueReusableCell("cell");

                if (cell == null)
                {
                    cell = new UITableViewCell();
                }

                cell.TextLabel.Text = _items.ElementAt(indexPath.Row);

                return cell;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                return _items.Count();
            }

            public string GetSelectedMap(NSIndexPath indexPath)
            {
                return _items.ElementAt(indexPath.Row);
            }
        }

        public MenuView(MapView mapView)
        {
            _mapView = mapView;
            Initialize();
        }

        private void Initialize()
        {
            BackgroundColor = UIColor.LightGray;

            _list = new UITableView();
            _list.BackgroundColor = UIColor.LightGray.ColorWithAlpha(0.5f);
            _list.Source = _source = new MenuTableViewSource();
            _list.AllowsMultipleSelection = false;
            Add(_list);

            _addNewMapView = new AddNewMapView();
            _addNewMapView.MapCreated += AddNewMapViewOnMapCreated;
            _addNewMapView.Canceled += AddNewMapViewOnCanceled;

            _load = new UIButton(UIButtonType.RoundedRect);
            _load.SetTitle("Load", UIControlState.Normal);
            _load.SetTitleColor(UIColor.Blue, UIControlState.Normal);
            _load.TouchUpInside += LoadOnTouchUpInside;
            Add(_load);

            _new = new UIButton(UIButtonType.RoundedRect);
            _new.SetTitle("New", UIControlState.Normal);
            _new.SetTitleColor(UIColor.Blue, UIControlState.Normal);
            _new.TouchUpInside += NewOnTouchUpInside;
            Add(_new);
        }

        private void AddNewMapViewOnCanceled()
        {
            _addNewMapView.RemoveFromSuperview();
            Add(_list);
        }

        private void AddNewMapViewOnMapCreated(string s)
        {
            FileSystemService.CreateNewMap(s);

            _source = new MenuTableViewSource();
            _list.Source = _source;

            _addNewMapView.RemoveFromSuperview();
            Add(_list);
        }

        private void NewOnTouchUpInside(object sender, EventArgs e)
        {
            _list.RemoveFromSuperview();
            Add(_addNewMapView);
        }

        private void LoadOnTouchUpInside(object sender, EventArgs eventArgs)
        {
            _mapView.Load(_source.GetSelectedMap(_list.IndexPathForSelectedRow));

            RemoveFromSuperview();
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            var dialogFrame = new CGRect(new CGPoint(0, 0), new CGSize(Frame.Width, Frame.Height - 50));

            _list.Frame = dialogFrame;
            _addNewMapView.Frame = dialogFrame;

            _load.Frame = new CGRect(new CGPoint(0, Frame.Height - 50), new CGSize(100, 50));

            _new.Frame = new CGRect(new CGPoint(Frame.Width - 100, Frame.Height - 50), new CGSize(100, 50));
        }
    }
}