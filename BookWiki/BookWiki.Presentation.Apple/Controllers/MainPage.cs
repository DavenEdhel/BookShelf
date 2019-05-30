using System;
using System.Linq;
using BookWiki.Core;
using BookWiki.Core.Files.FileSystemModels;
using BookWiki.Core.Files.PathModels;
using BookWiki.Core.LibraryModels;
using BookWiki.Presentation.Apple.Extentions;
using BookWiki.Presentation.Apple.Models;
using BookWiki.Presentation.Apple.Models.HotKeys;
using BookWiki.Presentation.Apple.Views;
using BookWiki.Presentation.Apple.Views.Controls;
using BookWiki.Presentation.Apple.Views.Main;
using CoreGraphics;
using Foundation;
using Keurig.IQ.Core.CrossCutting.Extensions;
using ObjCRuntime;
using UIKit;

namespace BookWiki.Presentation.Apple.Controllers
{
    public class MainPage : UIViewController
    {
        private Action _layout;

        private Keyboard _keyboard;

        private TabsCollectionView _tabsView;
        private ActionBarView _actionBarView;
        private ContentHolderView _contentHolderView;
        private NSObject _keyboardUp;
        private NSObject _keyboardDown;
        private Library _library;
        private SessionContext _session;

        private int _bottomOffset = 0;

        public override void ViewDidLoad()
        {
            _library = new Library(new UserFolderPath());
            _keyboard = new Keyboard(this);
            _session = new SessionContext(_library).Restore();

            Application.Run(_keyboard);

            _keyboardUp = NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.DidShowNotification, KeyboardIsUp);
            _keyboardDown = NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillHideNotification, KeyboardIsDown);

            base.ViewDidLoad();

            View.BackgroundColor = UIColor.White;

            Application.Instance.RegisterScheme(new HotKeyScheme(
                new HotKey(new Key("y"), () => _tabsView.SelectFiles()),
                new HotKey(new Key("u"), () => _tabsView.SelectTab(1)),
                new HotKey(new Key("i"), () => _tabsView.SelectTab(2)),
                new HotKey(new Key("o"), () => _tabsView.SelectTab(3)),
                new HotKey(new Key("p"), () => _tabsView.SelectTab(4))));

            _tabsView = new TabsCollectionView(_library);
            _tabsView.OnTabSelected += TabsViewOnOnTabSelected;
            _tabsView.Initialize(_session.OpenedContentTabs.Select(path => _library.Load(path)).ToArray());

            _contentHolderView = new ContentHolderView(data =>
            {
                if (data is IFileSystemNode node)
                {
                    return new FileSystemView(node, _tabsView, _library);
                }

                if (data is IArticle article)
                {
                    return new ArticleView(article);
                }

                if (data is INovel novel)
                {
                    var novelView = new NovelView(novel, _library);

                    return novelView;
                }

                if (data is IQuery searchQuery)
                {
                    return new SearchResultsView(searchQuery, _tabsView);
                }

                return new UIView() {BackgroundColor = UIColor.Black};
            });

            _actionBarView = new ActionBarView(_library, _contentHolderView, _session);
            _actionBarView.Search.OnSearchRequested += SearchOnOnSearchRequested;
            _actionBarView.SideMenuVisibilityChanged += SideMenuVisibilityChanged;

            var topSeparator = new UIView() {BackgroundColor = UIColor.LightGray};
            var verticalSeparator = new UIView() {BackgroundColor = UIColor.LightGray};
            var actionBarSeparator = new UIView() {BackgroundColor = UIColor.LightGray};

            View.AddSubviews(_tabsView, _actionBarView, topSeparator, verticalSeparator, actionBarSeparator, _contentHolderView);

            _layout = () =>
            {
                topSeparator.ChangeSize(View.Frame.Width, 1);
                topSeparator.ChangePosition(0, 20);

                if (_actionBarView.IsPanelHidden == false)
                {
                    _tabsView.Hidden = false;
                    _tabsView.ChangeWidth(200);
                    _tabsView.ChangeHeight(View.Frame.Height - 20 - _bottomOffset);
                    _tabsView.PositionToRight(View);
                    _tabsView.ChangeY(topSeparator.Frame.Bottom);

                    _actionBarView.ChangeY(topSeparator.Frame.Bottom);
                    _actionBarView.ChangeX(0);
                    _actionBarView.ChangeSize(View.Frame.Width - _tabsView.Frame.Width, 50);

                    actionBarSeparator.ChangeSize(_actionBarView.Frame.Width, 1);
                    actionBarSeparator.ChangePosition(0, _actionBarView.Frame.Bottom);

                    _contentHolderView.ChangeY(actionBarSeparator.Frame.Bottom);
                    _contentHolderView.ChangeX(0);
                    _contentHolderView.ChangeSize(_actionBarView.Frame.Width, View.Frame.Height - _actionBarView.Frame.Bottom - _bottomOffset);
                    _contentHolderView.LayoutSubviews();

                    verticalSeparator.Hidden = false;
                    verticalSeparator.ChangeSize(1, _tabsView.Frame.Height);
                    verticalSeparator.ChangePosition(_tabsView.Frame.Left, topSeparator.Frame.Bottom);
                }
                else
                {
                    _tabsView.Hidden = true;

                    _actionBarView.ChangeY(topSeparator.Frame.Bottom);
                    _actionBarView.ChangeX(0);
                    _actionBarView.ChangeSize(View.Frame.Width, 50);

                    actionBarSeparator.ChangeSize(_actionBarView.Frame.Width, 1);
                    actionBarSeparator.ChangePosition(0, _actionBarView.Frame.Bottom);

                    _contentHolderView.ChangeY(actionBarSeparator.Frame.Bottom);
                    _contentHolderView.ChangeX(0);
                    _contentHolderView.ChangeSize(_actionBarView.Frame.Width, View.Frame.Height - _actionBarView.Frame.Bottom - _bottomOffset);
                    _contentHolderView.LayoutSubviews();

                    verticalSeparator.Hidden = true;
                }
            };

            _layout();
        }

        private void SideMenuVisibilityChanged(bool obj)
        {
            _layout();
        }

        private void TabsViewOnOnTabSelected(object data)
        {
            _contentHolderView.Render(data);
        }

        private void SearchOnOnSearchRequested(IQuery searchQuery)
        {
            _tabsView.ShowSearchResult(searchQuery);
        }

        private void KeyboardIsDown(NSNotification obj)
        {
            _bottomOffset = 0;

            Application.Instance.ToViewMode();

            _layout();
        }

        private void KeyboardIsUp(NSNotification obj)
        {
            _bottomOffset = 55;

            Application.Instance.ToEditMode();

            _layout();
        }

        public override void ViewWillLayoutSubviews()
        {
            base.ViewWillLayoutSubviews();

            _layout();
        }

        [Export(Keyboard.KeyCommandMethodName)]
        private void KeyCommand(UIKeyCommand cmd)
        {
            _keyboard.CastTo<IKeyPressReceiver>().ProcessKey(cmd);
        }

        public void StoreData()
        {
            _session.Store(_tabsView, _actionBarView);
            _library.Save();
        }
    }
}