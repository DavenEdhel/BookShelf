using System;
using CoreGraphics;
using UIKit;

namespace BookWiki.Presentation.Apple.Views.Common
{
    public class SpaceSeparatorView : View
    {
        private readonly nfloat _height;

        public SpaceSeparatorView(nfloat height)
        {
            _height = height;

            Initialize();
        }

        private void Initialize()
        {
            BackgroundColor = UIColor.Clear;
        }

        public override CGSize SizeThatFits(CGSize size)
        {
            return new CGSize(size.Width, _height);
        }
    }
}