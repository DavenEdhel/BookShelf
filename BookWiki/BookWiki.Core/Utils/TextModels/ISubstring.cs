namespace BookWiki.Core.Utils.TextModels
{
    public interface ISubstring
    {
        string Text { get; }

        int StartIndex { get; }

        int EndIndex { get; }
    }
}