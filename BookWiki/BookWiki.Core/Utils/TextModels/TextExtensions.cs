using BookWiki.Core.Search;
using Keurig.IQ.Core.CrossCutting.Extensions;

namespace BookWiki.Core.Utils.TextModels
{
    public static class TextExtensions
    {
        public static int Length(this ITextRange tr)
        {
            return tr.CastTo<IRange>().Length;
        }

        public static ISequence<ITextRange> SplitBy(this IText text, char c)
        {
            var content = text.PlainText;

            return new RunOnceSequence<ITextRange>(new TextRangeSequence(content, c));
        }
    }
}