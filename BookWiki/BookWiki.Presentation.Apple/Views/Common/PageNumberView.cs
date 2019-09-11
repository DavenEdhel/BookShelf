using System;
using UIKit;

namespace BookWiki.Presentation.Apple.Views.Common
{
    public class PageNumberView : UILabel
    {
        private UITextView _textView;

        public PageNumberView()
        {
            Initialize();
        }

        private void Initialize()
        {
            Font = UIFont.BoldSystemFontOfSize(20);
            TextAlignment = UITextAlignment.Right;
            TextColor = UIColor.LightGray;
        }

        public void BindWith(UITextView textView)
        {
            if (_textView != null)
            {
                _textView.Scrolled -= TextViewOnScrolled;
                _textView.Changed -= TextViewOnChanged;
            }

            _textView = textView;

            _textView.Scrolled += TextViewOnScrolled;
            _textView.Changed += TextViewOnChanged;

            UpdatePaging();
        }

        private void TextViewOnChanged(object sender, EventArgs e)
        {
            UpdatePaging();
        }

        private void TextViewOnScrolled(object sender, EventArgs e)
        {
            UpdatePaging();
        }

        private void UpdatePaging()
        {
            var pageSize = 1120;

            var totalPages = (int)(_textView.ContentSize.Height / pageSize) + 1;

            var currentPage = (int)(_textView.ContentOffset.Y / pageSize) + 1;

            Text = $"{currentPage} из {totalPages}";
        }
    }
}