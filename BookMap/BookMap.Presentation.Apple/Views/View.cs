using System;
using UIKit;

namespace BookMap.Presentation.Apple.Views
{
    public class View : UIView
    {
        protected Action Layout = () => { };

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            Layout();
        }
    }
}