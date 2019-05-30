using CoreGraphics;
using UIKit;

namespace BookWiki.Presentation.Apple.Views
{
    public class HorizontalSeparatorView : View
    {
        public HorizontalSeparatorView()
        {
            Initialize();
        }

        private void Initialize()
        {
            BackgroundColor = UIColor.LightGray;
        }

        public override CGSize SizeThatFits(CGSize size)
        {
            return new CGSize(size.Width, 1);
        }
    }
}