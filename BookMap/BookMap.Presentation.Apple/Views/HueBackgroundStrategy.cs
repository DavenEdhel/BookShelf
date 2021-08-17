using System.Linq;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using UIKit;

namespace BookMap.Presentation.Apple.Views
{
    public class HueBackgroundStrategy : BackgroundStrategy
    {
        private readonly ColorPart _lockedPart;
        private CAGradientLayer _gradient;
        private UIColor _bottom;
        private UIColor _top;

        private UIColor[] _colors = new UIColor[]
        {
            UIColor.FromHSB(0.0f, 0.001f, 0.001f),
            UIColor.FromHSB(0.17f, 0.001f, 0.001f),
            UIColor.FromHSB(0.318f, 0.001f, 0.001f),
            UIColor.FromHSB(0.465f, 0.001f, 0.001f),
            UIColor.FromHSB(0.613f, 0.001f, 0.001f),
            UIColor.FromHSB(0.858f, 0.001f, 0.001f),
            UIColor.FromHSB(1f, 0.001f, 0.001f)
        };

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

        public override void ApplyActiveColor(UIColor color)
        {
            for (int i = 0; i < _colors.Length; i++)
            {
                var c = _colors[i];

                _colors[i] = color.SetTo(ColorPart.H, (float)c.ExtractPercentage(ColorPart.H));
            }

            Item.SetNeedsLayout();
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
            _gradient.Locations = new NSNumber[]
            {
                0.0f, 0.17f, 0.318f, 0.465f, 0.613f, 0.858f, 1f
            };
            _gradient.Colors = _colors.Select(x => x.CGColor).Reverse().ToArray();
        }
    }
}