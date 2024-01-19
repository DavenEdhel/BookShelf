using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
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
using System.Windows.Threading;
using BookWiki.Core;
using BookWiki.Core.Articles;
using BookWiki.Core.Files.FileSystemModels;
using BookWiki.Core.Files.PathModels;
using BookWiki.Core.Utils;
using BookWiki.Core.Utils.TextModels;
using BookWiki.Presentation.Wpf.Models;
using BookWiki.Presentation.Wpf.Views;
using Keurig.IQ.Core.CrossCutting.Extensions;

namespace BookWiki.Presentation.Wpf
{
    /// <summary>
    /// Interaction logic for AllArticlesWindow.xaml
    /// </summary>
    public partial class AllArticlesWindow : Window
    {
        private TagSearchResult[] _tags;
        private TagsSuggestionsBehavior _tagsSearch;
        private ArticlesScopeBehavior _articlesScopeBehavior;

        private readonly CompositeDisposable _scope = new CompositeDisposable();

        public AllArticlesWindow()
        {
            InitializeComponent();

            Initialize();

            SetAllArticlesAsync();
        }

        public AllArticlesWindow(string query)
        {
            InitializeComponent();

            Initialize();

            SetQuery(query);
        }

        private void Initialize()
        {
            _articlesScopeBehavior = new ArticlesScopeBehavior(
                GlobalScope,
                BookScope,
                ChapterScope,
                new ArticlesScopeOnFileSystem(BooksApplication.Instance.CurrentNovel, BooksApplication.Instance.RootPath)
            ).InScopeOf(_scope);

            foreach (var articlesScopeView in _articlesScopeBehavior.Views)
            {
                new TagsSuggestionsBehavior(articlesScopeView.QueryBox, Suggestions, _articlesScopeBehavior).InScopeOf(_scope);
            }

            _tagsSearch = new TagsSuggestionsBehavior(QueryBox, Suggestions, _articlesScopeBehavior).InScopeOf(_scope);

            _articlesScopeBehavior.ScopeChanged.Subscribe(_ => SetArticlesAsync()).InScopeOf(_scope);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            _scope.Dispose();

            _articlesScopeBehavior.Save();

            base.OnClosing(e);
        }

        public void FocusOnQuery()
        {
            QueryBox.Focus();
            if (string.IsNullOrWhiteSpace(QueryBox.Text) == false)
            {
                QueryBox.Select(QueryBox.Text.Length, 0);
            }
        }

        private void QueryBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            SetArticlesAsync();
        }

        private void SetArticlesAsync()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() => ArticlesView.Set(BooksApplication.Instance.Articles.Search(QueryBox.Text, _articlesScopeBehavior.Scope))));
        }

        private void SetAllArticlesAsync()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() => ArticlesView.Set(BooksApplication.Instance.Articles.Search("*", _articlesScopeBehavior.Scope))));
        }

        private void QueryBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
                return;
            }

            if (e.Key == Key.Enter && e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control))
            {
                var article = BooksApplication.Instance.Articles.New();
                BooksApplication.Instance.OpenArticle(article.Source, fullscreen: true).InitWith(QueryBox.Text.CapitalizeFirstLetter(), _articlesScopeBehavior.Scope);
                Close();
                return;
            }

            if (e.Key == Key.Enter)
            {
                if (ArticlesView.NothingFound)
                {
                    var article = BooksApplication.Instance.Articles.New();
                    BooksApplication.Instance.OpenArticle(article.Source, fullscreen: true).InitWith(QueryBox.Text.CapitalizeFirstLetter(), _articlesScopeBehavior.Scope);
                    Close();
                }
                else if (ArticlesView.NothingIsSelected)
                {
                    ArticlesView.NavigateToFirst();
                    Close();
                }
                else if (ArticlesView.AtLeastOneSelected)
                {
                    ArticlesView.NavigateToSelected();
                    Close();
                }
                else if (ArticlesView.OnlyOneExist)
                {
                    ArticlesView.NavigateToFirst();
                    Close();
                }
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
            BooksApplication.Instance.OpenArticle(BooksApplication.Instance.Articles.New().Source, fullscreen: true).InitWith(QueryBox.Text.CapitalizeFirstLetter(), _articlesScopeBehavior.Scope);

            Close();
        }

        public void SetQuery(string search)
        {
            QueryBox.Text = search;
            SetArticlesAsync();
            _tagsSearch.FillTags(BooksApplication.Instance.Articles.Tags);
        }
    }
}
