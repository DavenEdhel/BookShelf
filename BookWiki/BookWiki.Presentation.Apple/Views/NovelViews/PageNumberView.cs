using System;
using System.Collections;
using BookWiki.Core.Files.FileModels;
using UIKit;

namespace BookWiki.Presentation.Apple.Views.Common
{
    public class PageNumberView : UILabel
    {
        private UITextView _textView;

        private string _pageMode = PageNumber.Pages;

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

        public void SetPageMode(string pageMode)
        {
            _pageMode = pageMode;
            
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
            if (_pageMode == PageNumber.Pages)
            {
                UpdatePagingForPages();
            }

            if (_pageMode == PageNumber.NotDiplay)
            {
                UpdatePagingForNotDisplay();
            }

            if (_pageMode == PageNumber.AuthorLists)
            {
                UpdatePagingForAuthorLists();
            }

            if (_pageMode == PageNumber.Characters)
            {
                UpdatePagingForChars();
            }
        }

        private void UpdatePagingForAuthorLists()
        {
            var listSize = 40000;

            var currentSize = _textView.Text.Length;

            var als = (double)currentSize / (double)listSize;

            Text = $"{als:##.000} а.л.";
        }

        private void UpdatePagingForChars()
        {
            var currentSize = _textView.Text.Length;

            var ks = currentSize / 1000f;

            Text = $"{ks:###.0}k";
        }

        private void UpdatePagingForPages()
        {
            var pageSize = 1120;

            var totalPages = (int)(_textView.ContentSize.Height / pageSize) + 1;

            var currentPage = (int)(_textView.ContentOffset.Y / pageSize) + 1;

            Text = $"{currentPage} из {totalPages}";
        }

        private void UpdatePagingForNotDisplay()
        {
            Text = "  ";
        }
    }
}