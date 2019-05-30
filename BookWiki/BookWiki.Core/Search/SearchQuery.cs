using System.Linq;
using PureOop;

namespace BookWiki.Core.Search
{
    public class SearchQuery : IQuery
    {
        private readonly ILibrary _library;
        private readonly string _query;

        public SearchQuery(ILibrary library, string query)
        {
            _library = library;
            _query = query;
        }

        [JustOnce]
        public string Title => _query;

        [JustOnce]
        public string PlainText => _query.ToLowerInvariant();

        [JustOnce]
        public ISequence<SearchResult> Results
        {
            get
            {
                if (_query == "Все")
                {
                    return new RunOnceSequence<SearchResult>(new EnumerableSequence<SearchResult>(_library.Items.Select(x => new SearchResult(x)), _library.Items.Length));
                }
                else
                {
                    return new RunOnceSequence<SearchResult>(new SearchResultSequence(this, _library.Items));
                }
            }
        }
    }
}