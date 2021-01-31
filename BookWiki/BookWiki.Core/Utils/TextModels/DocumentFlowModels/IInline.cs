namespace BookWiki.Core
{
    public interface IInline
    {
        TextStyle TextStyle { get; }

        IText Text { get; set; }
    }
}