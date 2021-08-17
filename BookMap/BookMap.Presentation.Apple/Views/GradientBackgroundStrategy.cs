using CoreAnimation;
using CoreGraphics;
using UIKit;

namespace BookMap.Presentation.Apple.Views
{
    public class GradientBackgroundStrategy : BackgroundStrategy
    {
        private readonly ColorPart _lockedPart;
        private CAGradientLayer _gradient;
        private UIColor _bottom;
        private UIColor _top;

        public UIColor Top
        {
            get { return _top; }
            set
            {
                _top = value;
                Item.SetNeedsLayout();
            }
        }

        public UIColor Bottom
        {
            get { return _bottom; }
            set
            {
                _bottom = value;
                Item.SetNeedsLayout();
            }
        }

        public GradientBackgroundStrategy(ColorPart lockedPart)
        {
            _lockedPart = lockedPart;

            if (lockedPart == ColorPart.S)
            {
                _top = UIColor.FromHSB(0, 1, 0);
                _bottom = UIColor.FromHSB(0, 0, 0);
            }
            else
            {
                _top = lockedPart.ExtractTopColor();
                _bottom = UIColor.Black;
            }
        }

        public override void ApplyActiveColor(UIColor color)
        {
            Top = color.SetTo(_lockedPart, 1);
            Bottom = color.SetTo(_lockedPart, 0);
        }

        public override void Initialize(UIImageView imageView)
        {
            Item = imageView;

            _gradient = new CAGradientLayer();

            Item.Layer.InsertSublayer(_gradient, 0);

            Render();
        }

        public override void Render()
        {
            _gradient.Frame = new CGRect(new CGPoint(0, 0), Item.Frame.Size);
            _gradient.Colors = new CGColor[]
            {
                Top.CGColor,
                Bottom.CGColor,
            };
        }
    }
}