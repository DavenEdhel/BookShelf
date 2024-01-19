using BookShelf.Presentation.Wpf.Views;

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
    }
}