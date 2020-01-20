using System;
using System.Linq;
using BookWiki.Core;
using BookWiki.Core.Files.FileSystemModels;
using BookWiki.Core.Files.PathModels;
using BookWiki.Core.LibraryModels;
using BookWiki.Core.MockDataModels;
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

        private TabCollectionView _tabView;
        private ActionBarView _actionBarView;
        private ContentHolderView _contentHolderView;
        private NSObject _keyboardUp;
        private NSObject _keyboardDown;
        private Library _library;
        private SessionContext _session;

        private int _bottomOffset = 0;

        public override void ViewDidLoad()
        {
            new LibraryInitializationOperation(new UserFolderPath()).Execute();

            _library = new Library(new UserFolderPath(), () => _actionBarView);
            _keyboard = new Keyboard(this);
            _session = new SessionContext(_library).Restore();

            Application.Run(_keyboard);

            _keyboardUp = NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.DidShowNotification, KeyboardIsUp);
            _keyboardDown = NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillHideNotification, KeyboardIsDown);

            base.ViewDidLoad();

            View.BackgroundColor = UIColor.White;

            Application.Instance.RegisterScheme(new HotKeyScheme(
                new HotKey(new Key("y"), () => _tabView.SelectFiles()),
                new HotKey(new Key("u"), () => _tabView.SelectTab(1)),
                new HotKey(new Key("i"), () => _tabView.SelectTab(2)),
                new HotKey(new Key("o"), () => _tabView.SelectTab(3)),
                new HotKey(new Key("p"), () => _tabView.SelectTab(4))));

            _tabView = new TabCollectionView(_library);
            _tabView.OnTabSelected += TabViewOnOnTabSelected;
            _tabView.Initialize(_session.OpenedContentTabs.Select(state => _library.Load(state.NovelPathToLoad)).ToArray());

            _contentHolderView = new ContentHolderView(() => _bottomOffset, data =>
            {
                if (data is IFileSystemNode node)
                {
                    return new FileSystemView(node, _tabView, _library);
                }

                if (data is IArticle article)
                {
                    return new ArticleView(article);
                }

                if (data is INovel novel)
                {
                    var novelView = new NovelView(novel, _library, _actionBarView);
                    novelView.SetScrollVisibility(_actionBarView.IsScrollHidden);

                    var editorState = _session.OpenedContentTabs.FirstOrDefault(x => x.NovelPathToLoad.EqualsTo(novel.Source));

                    if (editorState != null)
                    {
                        novelView.State = editorState;
                    }

                    return novelView;
                }

                if (data is IQuery searchQuery)
                {
                    return new SearchResultsView(searchQuery, _tabView);
                }

                return new UIView() {BackgroundColor = UIColor.Black};
            });

            _actionBarView = new ActionBarView(_library, _contentHolderView, _session);
            _actionBarView.Search.OnSearchRequested += SearchOnOnSearchRequested;
            _actionBarView.SideMenuVisibilityChanged += SideMenuVisibilityChanged;
            _actionBarView.ScrollVisibilityChanged += ScrollVisibilityChanged;

            var topSeparator = new UIView() {BackgroundColor = UIColor.LightGray};
            var verticalSeparator = new UIView() {BackgroundColor = UIColor.LightGray};
            var actionBarSeparator = new UIView() {BackgroundColor = UIColor.LightGray};

            View.AddSubviews(_tabView, _actionBarView, topSeparator, verticalSeparator, actionBarSeparator, _contentHolderView);

            _layout = () =>
            {
                topSeparator.ChangeSize(View.Frame.Width, 1);
                topSeparator.ChangePosition(0, 20);

                if (_actionBarView.IsPanelHidden == false)
                {
                    _tabView.Hidden = false;
                    _tabView.ChangeWidth(200);
                    _tabView.ChangeHeight(View.Frame.Height - 20 - _bottomOffset);
                    _tabView.PositionToRight(View);
                    _tabView.ChangeY(topSeparator.Frame.Bottom);

                    _actionBarView.ChangeY(topSeparator.Frame.Bottom);
                    _actionBarView.ChangeX(0);
                    _actionBarView.ChangeSize(View.Frame.Width - _tabView.Frame.Width, 50);

                    actionBarSeparator.ChangeSize(_actionBarView.Frame.Width, 1);
                    actionBarSeparator.ChangePosition(0, _actionBarView.Frame.Bottom);

                    _contentHolderView.ChangeY(actionBarSeparator.Frame.Bottom);
                    _contentHolderView.ChangeX(0);
                    _contentHolderView.ChangeSize(_actionBarView.Frame.Width, View.Frame.Height - _actionBarView.Frame.Bottom - _bottomOffset);
                    _contentHolderView.LayoutSubviews();

                    verticalSeparator.Hidden = false;
                    verticalSeparator.ChangeSize(1, _tabView.Frame.Height);
                    verticalSeparator.ChangePosition(_tabView.Frame.Left, topSeparator.Frame.Bottom);
                }
                else
                {
                    _tabView.Hidden = true;

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

        private void ScrollVisibilityChanged(bool obj)
        {
            if (_contentHolderView.Current is NovelView novelView)
            {
                novelView.SetScrollVisibility(obj);
            }
        }

        private void SideMenuVisibilityChanged(bool obj)
        {
            _layout();
        }

        private void TabViewOnOnTabSelected(object data)
        {
            _contentHolderView.Render(data);
        }

        private void SearchOnOnSearchRequested(IQuery searchQuery)
        {
            _tabView.ShowSearchResult(searchQuery);
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
            _session.Store(_tabView, _actionBarView, _contentHolderView);
            _library.Save();
        }
    }
}