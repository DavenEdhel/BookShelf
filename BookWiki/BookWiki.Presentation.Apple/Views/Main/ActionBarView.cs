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

        private BooleanStatesView _hideShow;
        
        public PageSearchView Search => _search;

        private UIButton _save;
        private BooleanStatesView _hideShowScroll;
        private SeveralStatesView _changePagingMode;

        public SeveralStatesView PageMode => _changePagingMode;
        public BooleanStatesView PanelState => _hideShow;
        public BooleanStatesView ScrollState => _hideShowScroll;

        public ActionBarView(ILibrary library, ContentHolderView content, SessionContext session)
        {
            _library = library;
            _content = content;
            _session = session;
            Initialize();
        }

        private void Initialize()
        {
            _search = new PageSearchView(_library);
            Add(_search);

            _save = new UIButton(UIButtonType.RoundedRect);
            _save.SetTitleColor(UIColor.Black, UIControlState.Normal);
            _save.SetTitle("Save", UIControlState.Normal);
            _save.TouchUpInside += SaveOnTouchUpInside;
            Add(_save);

            _hideShow = new BooleanStatesView(on: "Panel", off: "No Panel");
            _hideShow.IsOff = _session.InterfaceSettings.IsSideBarHidden;
            Add(_hideShow);

            _hideShowScroll = new BooleanStatesView(on: "Scroll", off: "No Scroll");
            _hideShowScroll.IsOff = _session.InterfaceSettings.IsScrollHidden;
            Add(_hideShowScroll);

            _changePagingMode = new SeveralStatesView(PageNumberView.PageModes);
            _changePagingMode.CurrentIndex = _session.InterfaceSettings.PageModeIndex;
            Add(_changePagingMode);

            Layout = () =>
            {
                _hideShow.ChangeHeight(Frame.Height);
                _hideShow.ChangeWidth(70);
                _hideShow.PositionToRightAndCenterInside(this, 10);

                _hideShowScroll.ChangeHeight(Frame.Height);
                _hideShowScroll.ChangeWidth(70);
                _hideShowScroll.PositionToRightAndCenterInside(this, 10);
                _hideShowScroll.ChangeX(_hideShow.Frame.Left - 10 - _hideShowScroll.Frame.Width);

                _changePagingMode.ChangeHeight(Frame.Height);
                _changePagingMode.ChangeWidth(70);
                _changePagingMode.PositionToRightAndCenterInside(this, 10);
                _changePagingMode.ChangeX(_hideShowScroll.Frame.Left - 10 - _changePagingMode.Frame.Width);

                _save.SetSizeThatFits();
                _save.PositionToRightAndCenterInside(this, 10);
                _save.ChangeX(_changePagingMode.Frame.Left - 10 - _save.Frame.Width);
                
                _search.ChangeSize(Frame.Width - _hideShowScroll.Frame.Left - 20, Frame.Height);
                _search.ChangePosition(10, 0);
            };

            Layout();
        }

        private void SaveOnTouchUpInside(object sender, EventArgs e)
        {
            if (_content.Current is INovel novel)
            {
                _library.Update(novel);
                _library.Save();
            }
        }

        public bool IsUpToDate
        {
            set => InvokeOnMainThread(() => _save.Hidden = value);
        }
    }
}