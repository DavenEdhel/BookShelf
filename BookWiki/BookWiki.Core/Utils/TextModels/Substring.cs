namespace BookWiki.Core.Utils.TextModels
{
    public class Substring : ISubstring
    {
        public string Text { get; }
        public int StartIndex { get; }
        public int EndIndex { get; }

        public Substring(string text, int startIndex, int endIndex)
        {
            Text = text;
            StartIndex = startIndex;
            EndIndex = endIndex;
        }
    }
}