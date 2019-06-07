﻿using BookWiki.Core.Search;

namespace BookWiki.Core.Utils.TextModels
{
    public static class TextExtensions
    {
        public static ISequence<ITextRange> SplitBy(this IText text, char c)
        {
            var content = text.PlainText;

            return new RunOnceSequence<ITextRange>(new TextRangeSequence(content, c));
        }

        public static RangeOverlap In(this IRange range, IRange outerRange)
        {
            var startInRange = range.Start().In(outerRange);
            var endInRange = range.End().In(outerRange);

            if (startInRange && endInRange)
            {
                return RangeOverlap.Completely;
            }

            if (startInRange || endInRange)
            {
                return RangeOverlap.Partially;
            }

            return RangeOverlap.No;
        }

        public static bool In(this int x, IRange range)
        {
            if (x >= range.Offset && x <= range.End())
            {
                return true;
            }

            return false;
        }

        public static int Start(this IRange range) => range.Offset;

        public static int End(this IRange range) => range.Offset + range.Length;
    }
}