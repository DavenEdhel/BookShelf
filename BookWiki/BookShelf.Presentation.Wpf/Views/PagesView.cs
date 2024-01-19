using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using BookWiki.Core.Files.FileModels;
using BookWiki.Core.Utils;

namespace BookWiki.Presentation.Wpf
{
    public class PagesView : TextBlock
    {
        private string _pageMode = PageNumber.Pages;

        public PagesView()
        {
            Text = "0";
        }

        public RichTextBox Novel { get; set; }

        public ScrollViewer Scroll { get; set; }

        public void Start()
        {
            Novel.TextChanged += Content_OnTextChanged;
            Scroll.ScrollChanged += Scroll_OnScrollChanged;
            BooksApplication.Instance.PageConfig.Changed += PageConfigOnChanged;
            PageConfigOnChanged(BooksApplication.Instance.PageConfig.Current);
        }

        public void Stop()
        {
            Novel.TextChanged -= Content_OnTextChanged;
            Scroll.ScrollChanged -= Scroll_OnScrollChanged;
            BooksApplication.Instance.PageConfig.Changed -= PageConfigOnChanged;
        }

        private void Content_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            UpdatePaging();
        }

        private void Scroll_OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            TryUpdatePagingWhenScrolling();
        }

        private void PageConfigOnChanged(UserInterfaceSettings obj)
        {
            ApplyNewConfig(obj);
        }

        public void TryUpdatePagingWhenScrolling()
        {
            if (_pageMode == PageNumber.Pages)
            {
                UpdatePagingForPages();
            }
        }

        public void ApplyNewConfig(UserInterfaceSettings obj)
        {
            _pageMode = PageNumber.PageModes[obj.PageModeIndex];
            UpdatePaging();
        }

        private int Length => new TextRange(Novel.Document.ContentStart, Novel.Document.ContentEnd).Text.Length;

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);

            ChangeMode();
        }

        private void ChangeMode()
        {
            _pageMode = PageNumber.PageModes[(PageNumber.PageModes.IndexOf(x => x == _pageMode) + 1) % PageNumber.PageModes.Length];

            UpdatePaging();

            BooksApplication.Instance.PageConfig.SetDisplayMode(_pageMode);
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

            var als = (double)Length / (double)listSize;

            Text = $"{als:##.000} а.л.";
        }

        private void UpdatePagingForChars()
        {
            var ks = Length / 1000f;

            Text = $"{ks:###.0}k";
        }

        private void UpdatePagingForPages()
        {
            var pageSize = 1020;

            var totalPages = (int)(Novel.ActualHeight / pageSize) + 1;

            var currentPage = (int)(Scroll.ContentVerticalOffset / pageSize) + 1;

            Text = $"{currentPage} из {totalPages}";
        }

        private void UpdatePagingForNotDisplay()
        {
            Text = "     ";
        }
    }
}