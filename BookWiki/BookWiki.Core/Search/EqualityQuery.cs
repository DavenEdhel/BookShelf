namespace BookWiki.Core.Search
{
    public class EqualityQuery : IQuery
    {
        private readonly IQuery _query;

        public EqualityQuery(IQuery query)
        {
            _query = query;
        }

        public string PlainText => _query.PlainText;
        public string Title => _query.Title;
        public ISequence<SearchResult> Results => _query.Results;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj is IQuery query)
            {
                return query.PlainText.Equals(PlainText);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return (_query != null ? _query.GetHashCode() : 0);
        }
    }
}