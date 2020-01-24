using System.Collections.Generic;
using BookWiki.Core;
using BookWiki.Presentation.Apple.Views.Common;

namespace BookWiki.Presentation.Apple.Views.Controls
{
    public class LocalSearchItemCollection
    {
        private readonly IQuery _query;
        private readonly INovel _novel;
        private readonly EditTextView _textView;
        private readonly List<LocalSearchResultView> _results = new List<LocalSearchResultView>();
        private int _currentSelected = -1;
        private int _previous = -1;

        public LocalSearchItemCollection(IQuery query, INovel novel, EditTextView textView)
        {
            _query = query;
            _novel = novel;
            _textView = textView;
        }

        public void Apply()
        {
            foreach (var finding in new SearchResult(_novel, _query).Findings)
            {
                var resultHighlighter = new LocalSearchResultView(finding.Result, _textView);

                _results.Add(resultHighlighter);
            }
        }

        public void Remove()
        {
            foreach (var localSearchResultView in _results)
            {
                localSearchResultView.RemoveFromSuperview();
            }
        }

        public void SelectNext()
        {
            _previous = _currentSelected;
            _currentSelected = (_currentSelected + 1) % _results.Count;

            SelectCurrent();
        }

        public void SelectPrev()
        {
            _previous = _currentSelected;
            _currentSelected = (_currentSelected - 1 + _results.Count) % _results.Count;
        }

        private void SelectCurrent()
        {
            if (_previous != -1)
            {
                _results[_previous].IsSelected = false;
            }

            _results[_currentSelected].IsSelected = true;

            _textView.ScrollTo(_results[_currentSelected].Frame, animated: true);
        }
    }
}