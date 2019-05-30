using System;
using BookWiki.Core;
using BookWiki.Core.Search;
using BookWiki.Presentation.Apple.Controllers;
using BookWiki.Presentation.Apple.Extentions;
using BookWiki.Presentation.Apple.Models;
using BookWiki.Presentation.Apple.Models.HotKeys;
using UIKit;

namespace BookWiki.Presentation.Apple.Views.Common
{
    public class SearchView : View
    {
        private readonly ILibrary _library;
        private QueryView _input;

        public event Action<IQuery> OnSearchRequested = delegate {  };

        public SearchView(ILibrary library)
        {
            _library = library;
            Initialize();
        }

        private void Initialize()
        {
            Application.Instance.RegisterSchemeForViewMode(new HotKeyScheme(new HotKey(new Key("f"), StartFocus)));

            Application.Instance.RegisterSchemeForEditMode(
                new HotKeyScheme(
                    new HotKey(Key.Escape, LeaveFocus),
                    new HotKey(Key.Enter, MakeSearch).WithCommand()));

            _input = new QueryView(_library);
            _input.ShouldBecomeFirstResponderOnClick = true;
            Add(_input);

            Layout = () =>
            {
                _input.ChangeSize(Frame.Width, Frame.Height);
                _input.ChangePosition(0, 0);
            };

            Layout();
        }

        private void MakeSearch()
        {
            _input.ResignFirstResponder();

            OnSearchRequested(new EqualityQuery(new SearchQuery(_library, _input.QueryAsText)));

            // return search results correctly from query
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