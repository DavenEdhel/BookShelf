namespace BookWiki.Core
{
    public interface IParagraph
    {
        TextStyle FormattingStyle { get; }

        IInline[] Inlines { get; }
    }
}