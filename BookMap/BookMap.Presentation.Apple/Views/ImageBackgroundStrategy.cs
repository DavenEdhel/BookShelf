using UIKit;

namespace BookMap.Presentation.Apple.Views
{
    public class ImageBackgroundStrategy : BackgroundStrategy
    {
        private UIImage _backgroundImage;

        public UIImage BackgroundImage
        {
            get { return _backgroundImage; }
            set
            {
                _backgroundImage = value;
                Item.Image = value;
                Item.SetNeedsLayout();
            }
        }

        public ImageBackgroundStrategy(UIImage image)
        {
            _backgroundImage = image;
        }

        public override void Initialize(UIImageView imageView)
        {
            Item = imageView;

            Item.Image = BackgroundImage;
            Item.ContentMode = UIViewContentMode.ScaleToFill;
        }
    }
}