using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Shapes;
using BookWiki.Core.Files.FileSystemModels;
using BookWiki.Core.Files.PathModels;
using BookWiki.Presentation.Wpf.Views;

namespace BookWiki.Presentation.Wpf
{
    /// <summary>
    /// Interaction logic for AllArticlesWindow.xaml
    /// </summary>
    public partial class AllArticlesWindow : Window
    {
        public AllArticlesWindow()
        {
            InitializeComponent();

            ArticlesView.Set(BookShelf.Instance.Articles.Search("*"));
        }

        public void FocusOnQuery()
        {
            QueryBox.Text = string.Empty;
            QueryBox.Focus();
        }

        private void QueryBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            ArticlesView.Set(BookShelf.Instance.Articles.Search(QueryBox.Text));
        }

        private void QueryBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
                return;
            }

            if (e.Key == Key.Enter)
            {
                ArticlesView.NavigateToSelected();
                Close();
            }
        }

        private void QueryBox_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down)
            {
                ArticlesView.Down();
            }

            if (e.Key == Key.Up)
            {
                ArticlesView.Up();
            }
        }

        private void NewArticle(object sender, RoutedEventArgs e)
        {
            BookShelf.Instance.OpenArticle(BookShelf.Instance.Articles.New().Source, fullscreen: true);

            Close();
        }

        public void SetQuery(string search)
        {
            QueryBox.Text = search;
            ArticlesView.Set(BookShelf.Instance.Articles.Search(QueryBox.Text));
        }
    }
}
