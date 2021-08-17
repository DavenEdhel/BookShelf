using System;
using System.Drawing;
using System.Linq;
using BookMap.Presentation.Apple.Extentions;
using BookMap.Presentation.Apple.Models;
using BookMap.Presentation.Apple.Services;
using CoreGraphics;
using Foundation;
using UIKit;

namespace BookMap.Presentation.Apple.Views
{
    public class SingleImageView : View
    {
        private UIImageView _active;

        private CGPoint _additionalOffset = new CGPoint(0, 0);
        private nfloat _lastScale = 1;

        private CGPoint _zoomLocation;

        private bool _moveInProgress = false;
        private bool _zoomInProgress = false;

        private readonly CoordinateSystem _coordinates = new CoordinateSystem()
        {
            DisableScrollingToNegatives = false
        };

        public SingleImageView()
        {
            Initialize();
        }

        public UIImage ImageToImport => _active.Image;
        public FrameDouble FrameDouble => _coordinates.CurrentWorld;

        private void Initialize()
        {
            _active = new UIImageView();
            Add(_active);

            var ok = new UIButton(UIButtonType.RoundedRect);
            ok.SetTitleColor(UIColor.Red, UIControlState.Normal);
            ok.SetTitle("Ok", UIControlState.Normal);
            ok.TouchUpInside += OkOnTouchUpInside;
            Add(ok);

            var cancel = new UIButton(UIButtonType.RoundedRect);
            cancel.SetTitleColor(UIColor.Red, UIControlState.Normal);
            cancel.SetTitle("Cancel", UIControlState.Normal);
            cancel.TouchUpInside += CancelOnTouchUpInside;
            Add(cancel);

            Layout = () =>
            {
                ok.Frame = new CGRect(Frame.Width / 2, 50, 100, 50);

                cancel.Frame = new CGRect(Frame.Width / 2, 100, 100, 50);
            };

            _zoom = new UIPinchGestureRecognizer(OnZoom)
            {
                AllowedTouchTypes = new NSNumber[] { 0 },
                ShouldRecognizeSimultaneously = ShouldRecognizeSimultaneously,
                Enabled = true
            };

            _move = new UIPanGestureRecognizer(OnMove)
            {
                AllowedTouchTypes = new NSNumber[] { 0 },
                ShouldRecognizeSimultaneously = ShouldRecognizeSimultaneously,
                Enabled = true
            };

            AddGestureRecognizer(_zoom);
            AddGestureRecognizer(_move);
        }

        public event Action OkClicked = delegate { };

        public event Action CancelClicked = delegate { };

        private void CancelOnTouchUpInside(object sender, EventArgs eventArgs)
        {
            CancelClicked();
        }

        private void OkOnTouchUpInside(object sender, EventArgs eventArgs)
        {
            OkClicked();
        }

        private UIPinchGestureRecognizer _zoom;
        private UIPanGestureRecognizer _move;

        private bool ShouldRecognizeSimultaneously(UIGestureRecognizer gestureRecognizer, UIGestureRecognizer otherGestureRecognizer)
        {
            return true;
        }

        private void OnMove(UIPanGestureRecognizer parameters)
        {
            if (parameters.State == UIGestureRecognizerState.Began)
            {
                _coordinates.Begin();

                _moveInProgress = true;
            }
            else if (parameters.State == UIGestureRecognizerState.Changed)
            {
                _additionalOffset = parameters.TranslationInView(this);

                //System.Diagnostics.Debug.WriteLine($"{_additionalOffset.X}; {_additionalOffset.Y};");

                if (_zoomInProgress == false)
                {
                    PositionImage();
                }
            }
            else if (parameters.State == UIGestureRecognizerState.Ended)
            {
                _coordinates.End();

                _additionalOffset = new CGPoint(0, 0);

                _moveInProgress = false;
            }
        }

        private void OnZoom(UIPinchGestureRecognizer parameters)
        {
            if (parameters.State == UIGestureRecognizerState.Began)
            {
                _coordinates.Begin();

                _zoomInProgress = true;

                _zoomLocation = parameters.LocationInView(this);
            }
            else if (parameters.State == UIGestureRecognizerState.Changed)
            {
                _lastScale = parameters.Scale;

                PositionImage();
            }
            else if (parameters.State == UIGestureRecognizerState.Ended)
            {
                _coordinates.End();

                _lastScale = 1;

                _zoomInProgress = false;
            }
        }

        public void PositionImage()
        {
            _coordinates.MoveAndScaleFrame(_lastScale, _zoomLocation.ToPoint(), _additionalOffset.ToPoint());

            _active.Frame = _coordinates.CurrentWorld.ToRect();
        }

        public void Reset()
        {
            PositionImage();
        }

        public void Load(UIImage image)
        {
            _active.Image = image;

            PositionImage();
        }
    }
}