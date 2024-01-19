using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BookWiki.Core;
using BookWiki.Core.Articles;
using Keurig.IQ.Core.CrossCutting.Extensions;

namespace BookWiki.Presentation.Wpf.Views
{
    /// <summary>
    /// Interaction logic for ArticleItemView.xaml
    /// </summary>
    public partial class ArticleItemView : UserControl
    {
        private bool _isSelected = false;
        private ArticleSearchResult _searchResult;

        public ArticleItemView()
        {
            InitializeComponent();
        }

        public ArticleItemView(ArticleSearchResult searchResult)
        {
            InitializeComponent();

            Set(searchResult);
        }

        public void Set(ArticleSearchResult searchResult)
        {
            _searchResult = searchResult;

            ArticleName.Text = $"{searchResult.Article.Name} ({searchResult.Score})";
            MatchedTags.Text = searchResult.MatchedTags.JoinStringsWithoutSkipping(" ");
            PartiallyMatchedTags.Text = searchResult.PartiallyMatchedTags.JoinStringsWithoutSkipping(" ");
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;

                if (_isSelected)
                {
                    Border.Visibility = Visibility.Visible;
                }
                else
                {
                    Border.Visibility = Visibility.Hidden;
                }
            }
        }

        private void UIElement_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            Open();

            BooksApplication.Instance.AllArticlesWindow?.Close();
        }

        public void Open()
        {
            BooksApplication.Instance.OpenArticle(_searchResult?.Article.Source);
        }
    }
}
