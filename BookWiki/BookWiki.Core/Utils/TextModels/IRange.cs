namespace BookWiki.Core.Utils.TextModels
{
    public interface IRange
    {
        int Length { get; }

        int Offset { get; }
    }
}