namespace BookWiki.Core
{
    public interface ITextInfo
    {
        ITextRange Range { get; }

        TextStyle Style { get; }
    }
}