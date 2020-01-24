using BookWiki.Core.Utils.TextModels;
using CoreGraphics;
using Foundation;
using UIKit;

namespace BookWiki.Presentation.Apple.Views.Common
{
    public class ErrorLineView : TextDecorationView
    {
        protected override void ApplyStyle(CGRect rangeRect)
        {
            BackgroundColor = UIColor.FromRGBA(1, 0, 0, 0.3f);

            Frame = new CGRect(x: rangeRect.X, y: rangeRect.Bottom - 3, width: rangeRect.Width, height: 1);
        }

        public ErrorLineView(NSRange nsRange, UITextView textView) : base(nsRange, textView)
        {
        }

        public ErrorLineView(IRange range, UITextView textView) : base(range, textView)
        {
        }
    }
}