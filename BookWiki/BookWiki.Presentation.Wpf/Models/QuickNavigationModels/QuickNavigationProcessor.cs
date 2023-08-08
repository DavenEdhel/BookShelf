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
            BookShelf.Instance.OpenArticlesSearch(focusedOnQuery: true);
        }
    }
}