namespace BookWiki.Core
{
    public interface ITextRange : IText
    {
        int Offset { get; }
    }
}