namespace BookWiki.Core
{
    public class TextInfo : ITextInfo
    {
        public TextInfo(ITextRange range, TextStyle style)
        {
            Range = range;
            Style = style;
        }

        public ITextRange Range { get; }

        public TextStyle Style { get; }
    }
}