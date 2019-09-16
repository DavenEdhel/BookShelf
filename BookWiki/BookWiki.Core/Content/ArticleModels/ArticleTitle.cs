namespace BookWiki.Core
{
    public class ArticleTitle
    {
        private readonly string _title;

        public ArticleTitle(string title)
        {
            _title = title;
        }

        public string PlainText => _title;

        public string TextWithLineBreak => _title + "\n";
    }
}