using System.Collections.Generic;
using System.Linq;
using BookWiki.Core.Search;

namespace BookWiki.Core
{
    public class SearchQuery : IQuery
    {
        private readonly ILibrary _library;
        private readonly string _query;
        private readonly IProperty<SearchResult[]> _result;

        public SearchQuery(ILibrary library, string query)
        {
            _library = library;
            _query = query;

            if (_query == "Все")
            {
                Results = new RunOnceSequence<SearchResult>(new EnumerableSequence<SearchResult>(_library.Items.Select(x => new SearchResult(x)), _library.Items.Length));
            }
            else
            {
                Results = new RunOnceSequence<SearchResult>(new SearchResultSequence(this, _library.Items));
            }
        }

        public string Title => _query;

        public string PlainText => _query.ToLowerInvariant();

        public ISequence<SearchResult> Results { get; }
    }
}