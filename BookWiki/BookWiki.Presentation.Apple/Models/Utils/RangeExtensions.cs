using BookWiki.Core.Utils.TextModels;
using Foundation;

namespace BookWiki.Presentation.Apple.Models.Utils
{
    public static class RangeExtensions
    {
        public static NSRange ToNsRange(this IRange range)
        {
            return new NSRange(range.Offset, range.Length);
        }
    }
}