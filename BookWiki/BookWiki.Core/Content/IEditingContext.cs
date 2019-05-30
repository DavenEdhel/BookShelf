namespace BookWiki.Core
{
    public interface IEditingContext
    {
        int CursorPosition { get; }
    }
}