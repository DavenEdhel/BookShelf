using BookWiki.Core.Search;

namespace BookWiki.Core
{
    public static class TextExtensions
    {
        public static ISequence<ITextRange> SplitBy(this IText text, char c)
        {
            var content = text.PlainText;

            return new RunOnceSequence<ITextRange>(new TextRangeSequence(content, c));
        }
    }
}