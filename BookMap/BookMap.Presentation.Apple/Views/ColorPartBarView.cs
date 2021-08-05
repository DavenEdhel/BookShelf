using System;
using CoreGraphics;
using Foundation;
using UIKit;

namespace BookMap.Presentation.Apple.Views
{
    public class ColorPartBarView : UIImageView
    {
        private readonly ColorPart _lockedPart;
        private readonly BackgroundStrategy _strategy;
        private readonly Func<UIColor, float, UIColor> _modifyColor;

        private UIColor _color = UIColor.Black;

        public event Action<UIColor, ColorPart> ColorChanged = delegate { };

        public ColorPart LockedPart => _lockedPart;

        public UIColor CurrentColor
        {
            get { return _currentColor; }
            set
            {
                _currentColor = value;
                ColorChanged(_currentColor, _lockedPart);
                UpdateLevel();
            }
        }

        public void NotifyColorChanged(UIColor currentlyActive)
        {
            _currentColor = currentlyActive;
            _strategy.ApplyActiveColor(currentlyActive);
            UpdateLevel();
        }

        public ColorPartBarView(ColorPart lockedPart, UIColor currentColor, BackgroundStrategy strategy, Func<UIColor, float, UIColor> modifyColor)
        {
            _currentColor = currentColor;
            _lockedPart = lockedPart;
            _strategy = strategy;
            _modifyColor = modifyColor;
            Initialize();
        }

        public static ColorPartBarView Gradient(ColorPart lockedPart, UIColor currentColor)
        {
            return new ColorPartBarView(lockedPart, currentColor, new GradientBackgroundStrategy(lockedPart), (color, delta) => color.Modify(lockedPart, delta));
        }

        public static ColorPartBarView Hue(UIColor currentColor)
        {
            return new ColorPartBarView(ColorPart.H, currentColor, new HueBackgroundStrategy(), (color, delta) => color.Modify(ColorPart.H, delta));
        }

        public static ColorPartBarView Image(ColorPart lockedPart, UIColor currentColor, UIImage image)
        {
            return new ColorPartBarView(lockedPart, currentColor, new ImageBackgroundStrategy(image), (color, delta) => color.Modify(lockedPart, delta));
        }

        private void Initialize()
        {
            UserInteractionEnabled = true;

            _strategy.Initialize(this);

            _level = new UIView();
            _level.Frame = new CGRect(new CGPoint(), new CGSize(Frame.Width, 10));
            _level.BackgroundColor = UIColor.Black;
            _level.Layer.BorderWidth = 1;
            _level.Layer.BorderColor = UIColor.Black.CGColor;
            Add(_level);

            UpdateLevel();
        }

        private CGPoint _firstLocation;
        private UIColor _currentColor;
        private UIView _level;

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            var touch = (UITouch)touches.AnyObject;

            _firstLocation = touch.LocationInView(this);
            _color = CurrentColor;

            base.TouchesBegan(touches, evt);
        }

        public override void TouchesMoved(NSSet touches, UIEvent evt)
        {
            base.TouchesMoved(touches, evt);

            var touch = (UITouch)touches.AnyObject;

            var secondLocation = touch.LocationInView(this);

            var height = Frame.Height;

            var delta = (secondLocation.Y - _firstLocation.Y) / height;

            CurrentColor = _modifyColor(_color, -(float)delta);
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(touches, evt);

            _firstLocation = new CGPoint(0, 0);
            _color = CurrentColor;
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            
            _strategy.Render();
            UpdateLevel();
        }

        private void UpdateLevel()
        {
            if (_level != null)
            {
                _level.BackgroundColor = CurrentColor;
                var y = (1 - CurrentColor.ExtractPercentage(_lockedPart)) * Frame.Height;
                _level.Frame = new CGRect(0, y, Frame.Width, 10);
            }
        }
    }
}