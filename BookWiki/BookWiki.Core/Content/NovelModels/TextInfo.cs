using BookWiki.Core.Utils.TextModels;

namespace BookWiki.Core
{
    public class TextInfo : ITextInfo
    {
        public TextInfo(IRange range, TextStyle style)
        {
            Range = range;
            Style = style;
        }

        public IRange Range { get; }

        public TextStyle Style { get; }
    }
}