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

namespace BookWiki.Presentation.Wpf.Views
{
    /// <summary>
    /// Interaction logic for ArticleItemView.xaml
    /// </summary>
    public partial class ArticleItemView : UserControl
    {
        private bool _isSelected = false;
        private Article _article;

        public ArticleItemView()
        {
            InitializeComponent();
        }

        public ArticleItemView(Article article)
        {
            _article = article;
            
            InitializeComponent();

            ArticleName.Text = article.Name;
        }

        public void Set(Article article)
        {
            _article = article;
            ArticleName.Text = article.Source.Name.PlainText;
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

            BookShelf.Instance.AllArticlesWindow?.Close();
        }

        public void Open()
        {
            BookShelf.Instance.OpenArticle(_article?.Source);
        }
    }
}
