using System;
using BookWiki.Core;
using BookWiki.Core.Search;
using BookWiki.Presentation.Apple.Controllers;
using BookWiki.Presentation.Apple.Extentions;
using BookWiki.Presentation.Apple.Models;
using BookWiki.Presentation.Apple.Models.HotKeys;

namespace BookWiki.Presentation.Apple.Views.Common
{
    public class PageSearchView : View
    {
        private readonly ILibrary _library;
        private QueryView _input;
        private HotKeyScheme _editingScheme;

        public event Action SearchRequested = delegate { };

        public event Action QueryChanged = delegate { };

        public IQuery Query => new EqualityQuery(new SearchQuery(_library, _input.QueryAsText));
        
        public PageSearchView(ILibrary library)
        {
            _library = library;
            Initialize();
        }

        private void Initialize()
        {
            Application.Instance.RegisterSchemeForViewMode(new HotKeyScheme(new HotKey(new Key("f"), StartFocus)));

            Application.Instance.RegisterSchemeForEditMode(
                new HotKeyScheme(
                    new HotKey(new Key("f"), StartFocus).WithControl(),
                    new HotKey(Key.Escape, LeaveFocus)));

            _editingScheme = new HotKeyScheme(new HotKey(Key.Enter, MakeSearch));

            _input = new QueryView(_library);
            _input.Changed += InputOnChanged;
            _input.ShouldBecomeFirstResponderOnClick = true;
            _input.OnFocused += InputOnFocused;
            _input.OnFocusLeaved += InputOnOnFocusLeaved;
            Add(_input);

            Layout = () =>
            {
                _input.ChangeSize(Frame.Width, Frame.Height);
                _input.ChangePosition(0, 0);
            };

            Layout();
        }

        private void InputOnOnFocusLeaved()
        {
            Application.Instance.UnregisterScheme(_editingScheme);
        }

        private void InputOnFocused()
        {
            Application.Instance.RegisterSchemeForEditMode(_editingScheme);
        }

        private void InputOnChanged()
        {
            if (_input.QueryAsText.Length > 3)
            {
                QueryChanged();
            }
        }

        private void MakeSearch()
        {
            if (_input.IsFirstResponder == false)
            {
                return;
            }

            _input.ResignFirstResponder();

            if (_input.QueryAsText.Length > 3)
            {
                SearchRequested();
            }
        }

        private void LeaveFocus()
        {
            _input.ResignFirstResponder();
        }

        private void StartFocus()
        {
            _input.BecomeFirstResponder();
        }
    }
}