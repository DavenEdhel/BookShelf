namespace BookWiki.Core
{
    public interface IFormattedContent
    {
        ISequence<ITextInfo> Format { get; }

        IText Content { get; }
    }
}