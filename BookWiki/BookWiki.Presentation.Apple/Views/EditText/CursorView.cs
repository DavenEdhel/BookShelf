using BookWiki.Presentation.Apple.Extentions;
using UIKit;

namespace BookWiki.Presentation.Apple.Views.Controls
{
    public class CursorView : View
    {
        public CursorView()
        {
            Initialize();
        }

        private void Initialize()
        {
            BackgroundColor = UIColor.DarkGray;

            this.ChangeSize(1, 30);
        }
    }
}