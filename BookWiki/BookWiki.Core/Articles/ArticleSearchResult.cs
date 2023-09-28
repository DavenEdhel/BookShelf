using System.Collections.Generic;

namespace BookWiki.Core.Articles
{
    public class ArticleSearchResult
    {
        public Article Article { get; set; }

        public int Score { get; set; }

        public List<string> MatchedTags { get; set; } = new List<string>();

        public List<string> PartiallyMatchedTags { get; set; } = new List<string>();
    }
}