using System;
using CoreGraphics;
using Foundation;
using UIKit;

namespace BookMap.Presentation.Apple.Views
{
    public class SizeBarView : UIView
    {
        private float _currentSize;

        public event Action<float> SizeChanged = delegate { };

        public float CurrentSize
        {
            get { return _currentSize; }
            set
            {
                _currentSize = value;
                if (_currentSize > MaxValue)
                {
                    _currentSize = MaxValue;
                }
                if (_currentSize < MinValue)
                {
                    _currentSize = MinValue;
                }

                SizeChanged(_currentSize);
                UpdateLevel();
            }
        }

        public float MinValue { get; set; } = 5;
        public float MaxValue { get; set; } = 10;
        public float Range => MaxValue - MinValue;

        public SizeBarView()
        {
            Initialize();
        }

        private void Initialize()
        {
            Layer.BorderWidth = 1f;
            Layer.BorderColor = UIColor.Black.CGColor;
            _level = new UIView();
            _level.Frame = new CGRect(new CGPoint(), new CGSize(Frame.Width, 10));
            _level.BackgroundColor = UIColor.Black;
            _level.Layer.BorderWidth = 1;
            _level.Layer.BorderColor = UIColor.Black.CGColor;
            Add(_level);

            UpdateLevel();
        }

        private CGPoint _firstLocation;
        private UIView _level;
        private float _size;

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            var touch = (UITouch)touches.AnyObject;

            _firstLocation = touch.LocationInView(this);
            _size = CurrentSize;

            base.TouchesBegan(touches, evt);
        }

        public override void TouchesMoved(NSSet touches, UIEvent evt)
        {
            base.TouchesMoved(touches, evt);

            var touch = (UITouch)touches.AnyObject;

            var secondLocation = touch.LocationInView(this);

            var height = Frame.Height;

            var delta = (secondLocation.Y - _firstLocation.Y) / height * Range;

            CurrentSize = _size - (float)delta;
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(touches, evt);

            _firstLocation = new CGPoint(0, 0);
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            UpdateLevel();
        }

        private void UpdateLevel()
        {
            if (_level != null)
            {
                var y = (1 - CurrentSize/Range) * Frame.Height;
                _level.Frame = new CGRect(0, y, Frame.Width, 10);
            }
        }
    }
}