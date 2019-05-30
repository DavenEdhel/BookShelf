namespace BookWiki.Core
{
    public interface ISearchableContent : IContent
    {
        ISequence<int> SentenceStartIndexes { get; }

        ISequence<int> ParagraphsStartIndexes { get; }
    }
}