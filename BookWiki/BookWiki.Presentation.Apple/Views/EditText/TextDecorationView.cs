using BookWiki.Core;
using BookWiki.Core.Utils.TextModels;
using BookWiki.Presentation.Apple.Models.Utils;
using CoreGraphics;
using Foundation;
using UIKit;

namespace BookWiki.Presentation.Apple.Views.Common
{
    public abstract class TextDecorationView : View
    {
        private readonly UITextView _textView;
        private readonly IRange _inner;

        public TextDecorationView(NSRange nsRange, UITextView textView) : this(textView, new NativeRange(nsRange))
        {
        }

        public TextDecorationView(IRange range, UITextView textView) : this(textView, range)
        {
        }

        private TextDecorationView(UITextView parent, IRange range)
        {
            _inner = range;
            _textView = parent;

            parent.Add(this);

            ApplyStyle(CalculateFrame());
        }

        private CGRect CalculateFrame()
        {
            _textView.LayoutManager.EnsureLayoutForTextContainer(_textView.TextContainer);

            var start = _textView.GetPosition(_textView.BeginningOfDocument, _inner.Offset);
            var endPosition = _textView.GetPosition(start, _inner.Length);

            var range = _textView.GetTextRange(start, endPosition);

            return _textView.GetFirstRectForRange(range);
        }

        protected abstract void ApplyStyle(CGRect rangeRect);

        public bool In(IRange scope)
        {
            return _inner.In(scope).PartiallyOrCompletely();
        }

        public bool Exact(IRange range)
        {
            return _inner.In(range) == RangeOverlap.Exact;
        }

        public void DumpToConsole()
        {
            System.Diagnostics.Debug.WriteLine(_inner.ToFormattedString() + " -> " + Frame.Bottom);
        }
    }
}