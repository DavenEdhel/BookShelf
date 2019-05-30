namespace BookWiki.Core
{
    public interface IProperty<TOut>
    {
        TOut Value { get; }
    }
}