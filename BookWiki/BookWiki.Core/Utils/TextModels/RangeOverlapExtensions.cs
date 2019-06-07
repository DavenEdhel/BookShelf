namespace BookWiki.Core
{
    public static class RangeOverlapExtensions
    {
        public static bool PartiallyOrCompletely(this RangeOverlap overlap) =>
            overlap == RangeOverlap.Partially || overlap == RangeOverlap.Completely;
    }
}