using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BookWiki.Core.Search
{
    public class SearchResultSequence : ISequence<SearchResult>
    {
        private readonly IQuery _query;
        private readonly IContent[] _articles;

        public Progress Completion { get; }

        public SearchResultSequence(IQuery query, IContent[] articles)
        {
            _query = query;
            _articles = articles;
            Completion = new Progress(articles.Length);
        }

        public IEnumerator<SearchResult> GetEnumerator()
        {
            Completion.Reset();

            var notFound = new List<IContent>();

            foreach (var article in _articles)
            {
                var articleIndex = article.Title.IndexOf(_query.PlainText);

                if (articleIndex != -1)
                {
                    Completion.Increment();

                    yield return new SearchResult(article, _query);
                }
                else
                {
                    notFound.Add(article);
                }
            }

            foreach (var article in notFound)
            {
                var probableSearchResult = new SearchResult(article, _query);

                if (probableSearchResult.Findings.Any())
                {
                    yield return probableSearchResult;

                    Completion.Increment();
                }
            }

            Completion.MarkCompleted();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}