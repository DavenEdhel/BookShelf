using System;
using System.Drawing;
using CoreGraphics;
using Foundation;
using UIKit;

namespace BookMap.Presentation.Apple.Views
{
    public class MakeMeasureWizardView : UIView
    {
        private Action _layout = () => { };

        private UIView _pointA;
        private UIView _pointB;
        private UILabel _message;
        private UITextField _textEdit;
        private UIButton _apply;
        private UIButton _tryAgain;
        private UIButton _cancel;

        public event Action<double, double> MeasurePerformed = delegate { };

        public MakeMeasureWizardView()
        {
            Initialize();
        }

        private void Initialize()
        {
            BackgroundColor = UIColor.Clear;

            _message = new UILabel();
            _message.Text = "Draw the line to make measure.";
            Add(_message);

            _textEdit = new UITextField();
            _textEdit.Layer.BorderWidth = 1;
            _textEdit.Layer.BorderColor = UIColor.Black.CGColor;
            _textEdit.TextAlignment = UITextAlignment.Center;
            _textEdit.KeyboardType = UIKeyboardType.PhonePad;

            _pointA = new UIView();
            _pointA.BackgroundColor = UIColor.Black;
            _pointA.Layer.CornerRadius = 5;
            Add(_pointA);
            
            _pointB = new UIView();
            _pointB.BackgroundColor = UIColor.Black;
            _pointB.Layer.CornerRadius = 5;
            Add(_pointB);

            _apply = new UIButton(UIButtonType.RoundedRect);
            _apply.SetTitleColor(UIColor.Blue, UIControlState.Normal);
            _apply.TouchUpInside += ApplyOnTouchUpInside;
            _apply.SetTitle("Apply", UIControlState.Normal);

            _tryAgain = new UIButton(UIButtonType.RoundedRect);
            _tryAgain.SetTitleColor(UIColor.Blue, UIControlState.Normal);
            _tryAgain.TouchUpInside += TryAgainOnTouchUpInside;
            _tryAgain.SetTitle("Try Again", UIControlState.Normal);

            _cancel = new UIButton(UIButtonType.RoundedRect);
            _cancel.SetTitleColor(UIColor.Blue, UIControlState.Normal);
            _cancel.TouchUpInside += CancelOnTouchUpInside;
            _cancel.SetTitle("Cancel", UIControlState.Normal);

            _layout = () =>
            {
                _message.Frame = new CGRect(new CGPoint(_message.CenterXToCenterOf(this, 0), 50), _message.SizeEqualToContent());

                _textEdit.Frame = new CGRect(new CGPoint(_message.Frame.Left, _message.Frame.Bottom + 10), new CGSize(_message.Frame.Width, 30));

                _apply.Frame = new CGRect(new CGPoint(_message.Frame.Left, _textEdit.Frame.Bottom + 10), _apply.SizeEqualToContent());

                _tryAgain.Frame = new CGRect(new CGPoint(_apply.Frame.Right, _apply.Frame.Top), _tryAgain.SizeEqualToContent());

                _cancel.Frame = new CGRect(new CGPoint(_tryAgain.Frame.Right, _apply.Frame.Top), _cancel.SizeEqualToContent());
            };
        }

        private void ApplyOnTouchUpInside(object sender, EventArgs eventArgs)
        {
            var x1 = _pointA.Frame.X + _pointA.Frame.Width/2;
            var y1 = _pointA.Frame.Y + _pointA.Frame.Height/2;
            var x2 = _pointB.Frame.X + _pointB.Frame.Width/2;
            var y2 = _pointB.Frame.Y + _pointB.Frame.Height/2;
            var dx = x2 - x1;
            var dy = y2 - y1;

            var d = Math.Sqrt(dx * dx + dy * dy);

            MeasurePerformed(d, double.Parse(_textEdit.Text));

            RemoveFromSuperview();
        }

        private void CancelOnTouchUpInside(object sender, EventArgs eventArgs)
        {
            RemoveFromSuperview();
        }

        private void TryAgainOnTouchUpInside(object sender, EventArgs eventArgs)
        {
            _message.Text = "Draw the line to make measure.";
            _pointA.Frame = new CGRect();
            _pointB.Frame = new CGRect();

            _textEdit.RemoveFromSuperview();
            _apply.RemoveFromSuperview();
            _cancel.RemoveFromSuperview();
            _tryAgain.RemoveFromSuperview();
        }

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            base.TouchesBegan(touches, evt);

            var touch = (UITouch)touches.AnyObject;

            if (touch.Type != UITouchType.Stylus)
            {
                return;
            }

            var check = touch.LocationInView(this);

            _pointA.Frame = new CGRect(check, new CGSize(10, 10));
        }

        public override void TouchesMoved(NSSet touches, UIEvent evt)
        {
            base.TouchesMoved(touches, evt);

            var touch = (UITouch)touches.AnyObject;

            if (touch.Type != UITouchType.Stylus)
            {
                return;
            }

            var next = touch.LocationInView(this);

            _pointB.Frame = new CGRect(next, new CGSize(10, 10));
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(touches, evt);

            var touch = (UITouch)touches.AnyObject;

            if (touch.Type != UITouchType.Stylus)
            {
                return;
            }

            var next = touch.LocationInView(this);

            _pointB.Frame = new CGRect(next, new CGSize(10, 10));

            _message.Text = "Enter the distance in meters.";

            Add(_tryAgain);
            Add(_cancel);
            Add(_apply);

            Add(_textEdit);
        }
        
        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            _layout();
        }
    }
}