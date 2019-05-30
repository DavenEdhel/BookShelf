namespace BookWiki.Core
{
    public class EmptySubstring : ITextRange
    {
        public ITextRange Substring(int offset, int length)
        {
            return new EmptySubstring();
        }

        public int Length { get; } = 0;
        public string PlainText { get; } = string.Empty;
        public int Offset { get; } = 0;
    }
}