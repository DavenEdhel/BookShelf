using System;
using CoreGraphics;
using UIKit;

namespace BookMap.Presentation.Apple.Views
{
    public class AddNewMapView : UIView
    {
        private UIButton _create;
        private UIButton _cancel;
        private UITextField _editText;

        public event Action<string> MapCreated = delegate { };
        public event Action Canceled = delegate { };

        public AddNewMapView()
        {
            Initialize();
        }

        private void Initialize()
        {
            _editText = new UITextField();
            _editText.Layer.BorderWidth = 1;
            _editText.Layer.BorderColor = UIColor.Black.CGColor;
            _editText.TextAlignment = UITextAlignment.Center;
            Add(_editText);

            _create = new UIButton(UIButtonType.RoundedRect);
            _create.SetTitle("Create", UIControlState.Normal);
            _create.SetTitleColor(UIColor.Blue, UIControlState.Normal);
            _create.TouchUpInside += CreateOnTouchUpInside;
            Add(_create);

            _cancel = new UIButton(UIButtonType.RoundedRect);
            _cancel.SetTitle("Cancel", UIControlState.Normal);
            _cancel.SetTitleColor(UIColor.Blue, UIControlState.Normal);
            _cancel.TouchUpInside += CancelOnTouchUpInside;
            Add(_cancel);
        }

        private void CancelOnTouchUpInside(object sender, EventArgs e)
        {
            Canceled();
        }

        private void CreateOnTouchUpInside(object sender, EventArgs e)
        {
            MapCreated(_editText.Text);
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            _editText.Frame = new CGRect(new CGPoint(0, 0), new CGSize(Frame.Width, 50));

            _create.Frame = new CGRect(new CGPoint(0, Frame.Height - 50), new CGSize(100, 50));

            _cancel.Frame = new CGRect(new CGPoint(Frame.Width - 100, Frame.Height - 50), new CGSize(100, 50));
        }
    }
}