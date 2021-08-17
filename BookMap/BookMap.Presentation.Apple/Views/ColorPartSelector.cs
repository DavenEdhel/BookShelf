using CoreGraphics;
using UIKit;

namespace BookMap.Presentation.Apple.Views
{
    public class ColorPartSelector : UIView
    {
        private readonly string _titleText;
        private UISlider _slider;
        private UILabel _title;

        public UISlider Slider => _slider;

        public ColorPartSelector(string titleText)
        {
            _titleText = titleText;
            Initialize();
        }

        private void Initialize()
        {
            _slider = new UISlider();
            _slider.MinValue = 0;
            _slider.MaxValue = 1;
            Add(_slider);

            _title = new UILabel();
            _title.TextColor = UIColor.White;
            _title.Text = _titleText;
            Add(_title);
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            _title.Frame = new CGRect(0, 0, 20, 20);

            _slider.Frame = new CGRect(25, 0, Frame.Width - 25, 20);
        }
    }
}