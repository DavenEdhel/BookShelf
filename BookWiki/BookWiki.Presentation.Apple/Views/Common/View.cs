using System;
using CoreGraphics;
using UIKit;

namespace BookWiki.Presentation.Apple.Views
{
    public class View : UIView
    {
        protected Action Layout = delegate { };

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            Layout();
        }

        public override CGSize SizeThatFits(CGSize size)
        {
            Layout();

            return Frame.Size;
        }
    }
}