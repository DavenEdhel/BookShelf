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

        public ErrorLineView(NSRange nsRange, UITextView textView)
        {
            _textView = textView;
            _inner = new NativeRange(nsRange);

            textView.Add(this);

            BackgroundColor = UIColor.FromRGBA(1, 0, 0, 0.3f);

            Frame = CalculateFrame();
        }

        private CGRect CalculateFrame()
        {
            var start = _textView.GetPosition(_textView.BeginningOfDocument, _inner.Offset);
            var endPosition = _textView.GetPosition(start, _inner.Length);

            var range = _textView.GetTextRange(start, endPosition);

            var rangeRect = _textView.GetFirstRectForRange(range);

            return new CGRect(x: rangeRect.X, y: rangeRect.Bottom, width: rangeRect.Width, height: 1);
        }

        public bool In(IRange scope)
        {
            return _inner.In(scope).PartiallyOrCompletely();
        }
    }
}