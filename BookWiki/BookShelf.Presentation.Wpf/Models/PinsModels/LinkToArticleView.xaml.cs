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
using BookMap.Presentation.Wpf;
using BookWiki.Core;
using BookWiki.Presentation.Wpf;

namespace BookShelf.Presentation.Wpf.Models.PinsModels
{
    /// <summary>
    /// Interaction logic for LinkToArticleView.xaml
    /// </summary>
    public partial class LinkToArticleView : UserControl
    {
        private readonly Article _article;

        public LinkToArticleView()
        {
            InitializeComponent();
        }

        public LinkToArticleView(Article article)
        {
            _article = article;
            InitializeComponent();

            ArticleName.Text = _article.Name;
        }

        private void OpenArticle(object sender, MouseButtonEventArgs e)
        {
            BooksApplication.Instance.OpenArticle(_article.Source);
        }
    }
}
