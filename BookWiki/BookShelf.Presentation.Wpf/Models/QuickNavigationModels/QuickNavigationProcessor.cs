using BookShelf.Presentation.Wpf.Views;
using BookWiki.Core.Files.PathModels;
using Keurig.IQ.Core.CrossCutting.Extensions;

namespace BookWiki.Presentation.Wpf.Models.QuickNavigationModels
{
    public class QuickNavigationProcessor
    {
        public void Process()
        {
            new QuickNavigationWindow().ShowDialog();
        }
    }

    public class QuickArticleProcessor
    {
        public void Process()
        {
            BooksApplication.Instance.OpenArticlesSearch(focusedOnQuery: true);
        }
    }

    public class QuickMapProcessor
    {
        public void Process()
        {
            new QuickMapNavigationWindow().ShowDialog();
        }

        public void TryOpenPreprocessed()
        {
            if (string.IsNullOrEmpty(BooksApplication.Instance.Scope.Book.Content.MapName))
            {
                new QuickMapNavigationWindow().ShowDialog();
            }
            else
            {
                BooksApplication.Instance.OpenMap(BooksApplication.Instance.Maps.MapsRoot.RelativePath(BooksApplication.Instance.RootPath).Down(BooksApplication.Instance.Scope.Book.Content.MapName + ".m"));
            }
        }
    }
}