using System;
using BookWiki.Core;
using BookWiki.Presentation.Apple.Controllers;
using BookWiki.Presentation.Apple.Extentions;
using BookWiki.Presentation.Apple.Views.Common;
using BookWiki.Presentation.Apple.Views.Main;
using UIKit;

namespace BookWiki.Presentation.Apple.Views
{
    public class ActionBarView : View
    {
        private SearchView _search;
        private readonly ILibrary _library;
        private readonly ContentHolderView _content;
        private readonly SessionContext _session;

        private UIButton _hideShow;

        public bool IsPanelHidden { get; private set; }

        public SearchView Search => _search;

        public Action<bool> SideMenuVisibilityChanged = delegate { };

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

            _search = new SearchView(_library);
            Add(_search);

            var save = new UIButton(UIButtonType.RoundedRect);
            save.SetTitleColor(UIColor.Black, UIControlState.Normal);
            save.SetTitle("Save", UIControlState.Normal);
            save.TouchUpInside += SaveOnTouchUpInside;
            Add(save);

            _hideShow = new UIButton(UIButtonType.RoundedRect);
            _hideShow.SetTitleColor(UIColor.Black, UIControlState.Normal);
            _hideShow.SetTitle(_session.InterfaceSettings.IsSideBarHidden ? "Show" : "Hide", UIControlState.Normal);
            _hideShow.TouchUpInside += HideShowOnTouchUpInside;
            Add(_hideShow);

            Layout = () =>
            {
                _hideShow.SetSizeThatFits();
                _hideShow.ChangeWidth(70);
                _hideShow.PositionToRightAndCenterInside(this, 10);

                save.SetSizeThatFits();
                save.PositionToRightAndCenterInside(this, (int)_hideShow.Frame.Width + 20);

                _search.ChangeSize(Frame.Width - save.Frame.Width - _hideShow.Frame.Width - 20, Frame.Height);
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

        private void SaveOnTouchUpInside(object sender, EventArgs e)
        {
            _library.Save();
        }
    }
}