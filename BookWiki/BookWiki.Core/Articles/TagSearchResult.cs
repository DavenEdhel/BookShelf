namespace BookWiki.Core.Articles
{
    public class TagSearchResult
    {
        public int Index { get; set; }

        public Tag Tag { get; set; }

        public int Score { get; set; }

        public int ClarificationScore { get; set; }
    }
}