namespace BookWiki.Core
{
    public interface IQuery
    {
        string PlainText { get; }

        string Title { get; }

        ISequence<SearchResult> Results { get; }

        bool Equals(object obj);

        int GetHashCode();
    }
}