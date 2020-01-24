using System;
using BookWiki.Core;
using BookWiki.Presentation.Apple.Controllers;
using BookWiki.Presentation.Apple.Extentions;
using BookWiki.Presentation.Apple.Views.Common;
using BookWiki.Presentation.Apple.Views.Controls;
using BookWiki.Presentation.Apple.Views.Main;
using UIKit;

namespace BookWiki.Presentation.Apple.Views
{
    public class ActionBarView : View, ISaveStatus
    {
        private PageSearchView _search;
        private readonly ILibrary _library;
        private readonly ContentHolderView _content;
        private readonly SessionContext _session;

        private UIButton _hideShow;

        public bool IsPanelHidden { get; private set; }
        public bool IsScrollHidden { get; private set; }

        public PageSearchView Search => _search;

        public Action<bool> SideMenuVisibilityChanged = delegate { };
        public Action<bool> ScrollVisibilityChanged = delegate { };
        private UIButton _save;
        private UIButton _hideShowScroll;

        public ActionBarView(ILibrary library, ContentHolderView content, SessionContext session)
        {
            _library = library;
            _content = content;
            _session = session;
            Initialize();
        }

        private void Initialize()
        {
            IsPanelHidden = _session.InterfaceSettings.IsSideBarHidden;
            IsScrollHidden = true;

            _search = new PageSearchView(_library);
            Add(_search);

            _save = new UIButton(UIButtonType.RoundedRect);
            _save.SetTitleColor(UIColor.Black, UIControlState.Normal);
            _save.SetTitle("Save", UIControlState.Normal);
            _save.TouchUpInside += SaveOnTouchUpInside;
            Add(_save);

            _hideShow = new UIButton(UIButtonType.RoundedRect);
            _hideShow.SetTitleColor(UIColor.Black, UIControlState.Normal);
            _hideShow.SetTitle(_session.InterfaceSettings.IsSideBarHidden ? "Show" : "Hide", UIControlState.Normal);
            _hideShow.TouchUpInside += HideShowOnTouchUpInside;
            Add(_hideShow);

            _hideShowScroll = new UIButton(UIButtonType.RoundedRect);
            _hideShowScroll.SetTitleColor(UIColor.Black, UIControlState.Normal);
            _hideShowScroll.SetTitle(IsScrollHidden ? "Show" : "Hide", UIControlState.Normal);
            _hideShowScroll.TouchUpInside += HideShowScrollOnTouchUpInside;
            Add(_hideShowScroll);

            Layout = () =>
            {
                _hideShow.SetSizeThatFits();
                _hideShow.ChangeWidth(70);
                _hideShow.PositionToRightAndCenterInside(this, 10);

                _hideShowScroll.SetSizeThatFits();
                _hideShowScroll.ChangeWidth(70);
                _hideShowScroll.PositionToRightAndCenterInside(this, 10);
                _hideShowScroll.ChangeX(_hideShow.Frame.Left - 10 - _hideShowScroll.Frame.Width);

                _save.SetSizeThatFits();
                _save.PositionToRightAndCenterInside(this, 10);
                _save.ChangeX(_hideShowScroll.Frame.Left - 10 - _save.Frame.Width);
                
                _search.ChangeSize(Frame.Width - _hideShowScroll.Frame.Left - 20, Frame.Height);
                _search.ChangePosition(10, 0);
            };

            Layout();
        }

        private void HideShowOnTouchUpInside(object sender, EventArgs e)
        {
            IsPanelHidden = !IsPanelHidden;

            _hideShow.SetTitle(IsPanelHidden ? "Show" : "Hide", UIControlState.Normal);

            SideMenuVisibilityChanged(IsPanelHidden);
        }

        private void HideShowScrollOnTouchUpInside(object sender, EventArgs e)
        {
            IsScrollHidden = !IsScrollHidden;

            _hideShowScroll.SetTitle(IsScrollHidden ? "Show" : "Hide", UIControlState.Normal);

            ScrollVisibilityChanged(IsScrollHidden);
        }

        private void SaveOnTouchUpInside(object sender, EventArgs e)
        {
            _library.Save();
        }

        public bool IsUpToDate
        {
            set => InvokeOnMainThread(() => _save.Hidden = value);
        }
    }
}