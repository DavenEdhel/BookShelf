using BookWiki.Core;
using BookWiki.Core.Utils.TextModels;

namespace BookWiki.Presentation.Apple.Models.Utils
{
    public static class Range
    {
        public static int Start(this IRange range) => range.Offset;

        public static int End(this IRange range) => range.Offset + range.Length;

        public static int EndV2(this IRange range) => range.Offset + range.Length - 1;

        public static int Middle(this IRange range) => range.Start() + (range.End() - range.Start()) / 2;

        public static string ToFormattedString(this IRange range)
        {
            return $"[{range.Start()},{range.End()}]";
        }

        public static IText GetPlainText(this IRange range, IText text)
        {
            return text.Substring(range.Offset, range.Length);
        }

        public static RangeOverlap InV2(this IRange range, IRange outerRange)
        {
            if (range.Start() == outerRange.Start() && range.End() == outerRange.End() - 1)
            {
                return RangeOverlap.Exact;
            }

            var startInRange = range.Start().InV2(outerRange);
            var endInRange = range.EndV2().InV2(outerRange);

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

        public static RangeOverlap In(this IRange range, IRange outerRange)
        {
            if (range.Start() == outerRange.Start() && range.End() == outerRange.End())
            {
                return RangeOverlap.Exact;
            }

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

        public static bool InV2(this int x, IRange range)
        {
            if (x >= range.Start() && x <= range.EndV2())
            {
                return true;
            }

            return false;
        }

        public static bool In(this int x, IRange range)
        {
            if (x >= range.Start() && x <= range.End())
            {
                return true;
            }

            return false;
        }
    }
}