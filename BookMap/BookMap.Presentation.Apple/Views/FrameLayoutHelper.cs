using System;
using CoreGraphics;
using UIKit;

namespace BookMap.Presentation.Apple.Views
{
    public static class FrameLayoutHelper
    {
        public static CGSize SizeEqualToContent(this UIView view)
        {
            return view.SizeThatFits(new CGSize(float.MaxValue, float.MaxValue));
        }

        public static CGPoint Center(this UIView superview, float xOffset, float yOffset)
        {
            return new CGPoint(superview.Frame.Width / 2 + xOffset, superview.Frame.Height / 2 + yOffset);
        }

        public static nfloat CenterXToCenterOf(this UIView view, UIView superview, float offset)
        {
            return superview.Frame.Width / 2 - view.SizeEqualToContent().Width / 2 + offset;
        }
    }
}