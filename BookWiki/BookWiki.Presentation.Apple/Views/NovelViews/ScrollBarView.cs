using System;
using CoreGraphics;
using Foundation;
using Keurig.IQ.Core.CrossCutting.Extensions;
using UIKit;

namespace BookWiki.Presentation.Apple.Views.Common
{
    public class ScrollBarView : View
    {
        private readonly EditTextView _editText;

        private UIView _pan;
        private CGPoint _startTouch;
        private nfloat _current;

        private bool _isDragMode = false;

        public ScrollBarView(EditTextView editText)
        {
            _editText = editText;
            _editText.Changed += EditTextOnChanged;
            _editText.DidScrollEnd += EditTextOnDidScrollEnd;
            _editText.Scrolled += EditTextOnScrolled;
            
            Initialize();

            void Initialize()
            {
                BackgroundColor = UIColor.FromHSB(0, 0, 0.95f);

                _pan = new UIView();
                _pan.Layer.BorderColor = UIColor.LightGray.CGColor;
                _pan.Layer.BorderWidth = 1;
                Add(_pan);

                Layout = () =>
                {
                    _pan.Frame = new CGRect(0, _pan.Frame.Y, Frame.Width, Frame.Height/2);

                    UpdateSize();
                };
            }
        }

        private void EditTextOnScrolled(object sender, EventArgs e)
        {
            UpdatePosition();
        }

        private void EditTextOnDidScrollEnd()
        {
            UpdatePosition();
        }

        private void EditTextOnChanged(object sender, EventArgs e)
        {
            UpdateSize();
            UpdatePosition();
        }

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            base.TouchesBegan(touches, evt);

            var touch = evt.AllTouches.AnyObject.CastTo<UITouch>();

            _startTouch = touch.GetPreciseLocation(this);

            _current = _pan.Frame.Y;

            _isDragMode = _pan.Frame.IntersectsWith(new CGRect(_startTouch.X - 5, _startTouch.Y - 5, 10, 10));
        }

        public override void TouchesMoved(NSSet touches, UIEvent evt)
        {
            base.TouchesMoved(touches, evt);

            if (_isDragMode)
            {
                var touch = evt.AllTouches.AnyObject.CastTo<UITouch>();

                var current = touch.GetPreciseLocation(this);

                SetCursorY(_current + (current.Y - _startTouch.Y));

                UpdateScrollPosition(animated: false);
            }
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(touches, evt);

            CompleteTouch(touches, false);
        }

        public override void TouchesCancelled(NSSet touches, UIEvent evt)
        {
            base.TouchesCancelled(touches, evt);

            CompleteTouch(touches, true);
        }

        private void CompleteTouch(NSSet touches, bool isCancelled)
        {
            if (_isDragMode)
            {
                _current = _pan.Frame.Y;

                SetCursorY(_current, animated: false, updateScrollPosition: true);
            }
            else
            {
                if (isCancelled == false)
                {
                    var touch = touches.AnyObject.CastTo<UITouch>();

                    var current = touch.GetPreciseLocation(this);

                    SetCursorY(current.Y, animated: true, updateScrollPosition: true);
                }
            }
        }

        private void UpdatePosition()
        {
            var current = _editText.ContentOffset.Y / (_editText.ContentSize.Height - _editText.Frame.Height);

            var available = Frame.Height - _pan.Frame.Height;

            var resulted = current * available;

            SetCursorY(resulted, animated: true);
        }

        private void SetCursorY(nfloat value, bool animated = false, bool updateScrollPosition = false)
        {
            if (value < 0)
            {
                value = 0;
            }

            if (value + _pan.Frame.Height > Frame.Height)
            {
                value = Frame.Height - _pan.Frame.Height;
            }

            if (animated)
            {
                Animate(0.3, () =>
                {
                    _pan.Frame = new CGRect(0, value, Frame.Width, _pan.Frame.Height);
                });
            }
            else
            {
                _pan.Frame = new CGRect(0, value, Frame.Width, _pan.Frame.Height);
            }

            if (updateScrollPosition)
            {
                UpdateScrollPosition(animated: true);
            }
        }

        private void UpdateScrollPosition(bool animated = false)
        {
            var percent = _pan.Frame.Y / (Frame.Height - _pan.Frame.Height);

            var y = percent * (_editText.ContentSize.Height - _editText.Frame.Height);

            _editText.SetContentOffset(new CGPoint(0, y), animated);
        }

        private void UpdateSize()
        {
            var p = _editText.Frame.Height / _editText.ContentSize.Height;

            var percent = p > 1 ? 1 : p;

            var h = Frame.Height * percent;

            _pan.Frame = new CGRect(0, _pan.Frame.Y, Frame.Width, h);
        }
    }
}