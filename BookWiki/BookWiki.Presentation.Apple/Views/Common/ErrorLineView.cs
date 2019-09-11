using BookWiki.Core;
using BookWiki.Core.Utils.TextModels;
using BookWiki.Presentation.Apple.Models.Utils;
using CoreGraphics;
using Foundation;
using UIKit;

namespace BookWiki.Presentation.Apple.Views.Common
{
    public class ErrorLineView : View
    {
        private readonly UITextView _textView;
        private readonly IRange _inner;

        public ErrorLineView(NSRange nsRange, UITextView textView) : this(textView, new NativeRange(nsRange))
        {
        }

        public ErrorLineView(IRange range, UITextView textView) : this(textView, range)
        {
        }

        private ErrorLineView(UITextView parent, IRange range)
        {
            _inner = range;
            _textView = parent;

            parent.Add(this);

            BackgroundColor = UIColor.FromRGBA(1, 0, 0, 0.3f);

            Frame = CalculateFrame();
        }

        private CGRect CalculateFrame()
        {
            _textView.LayoutManager.EnsureLayoutForTextContainer(_textView.TextContainer);

            var start = _textView.GetPosition(_textView.BeginningOfDocument, _inner.Offset);
            var endPosition = _textView.GetPosition(start, _inner.Length);

            var range = _textView.GetTextRange(start, endPosition);

            var rangeRect = _textView.GetFirstRectForRange(range);

            return new CGRect(x: rangeRect.X, y: rangeRect.Bottom - 3, width: rangeRect.Width, height: 1);
        }

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