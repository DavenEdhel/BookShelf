using BookWiki.Core.Utils.TextModels;
using CoreGraphics;
using Foundation;
using UIKit;

namespace BookWiki.Presentation.Apple.Views.Common
{
    public class LocalSearchResultView : TextDecorationView
    {
        public LocalSearchResultView(NSRange nsRange, UITextView textView) : base(nsRange, textView)
        {
        }

        public LocalSearchResultView(IRange range, UITextView textView) : base(range, textView)
        {
        }

        public bool IsSelected
        {
            set
            {
                if (value)
                {
                    BackgroundColor = UIColor.Orange.ColorWithAlpha(0.2f);
                }
                else
                {
                    BackgroundColor = UIColor.Yellow.ColorWithAlpha(0.2f);
                }
            }
        }

        protected override void ApplyStyle(CGRect rangeRect)
        {
            BackgroundColor = UIColor.Yellow.ColorWithAlpha(0.2f);
            Frame = rangeRect;
        }
    }
}