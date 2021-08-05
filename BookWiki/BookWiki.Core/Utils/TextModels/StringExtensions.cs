using BookWiki.Core.Utils.TextModels;

namespace BookWiki.Core
{
    public static class StringExtensions
    {
        public static string Substring(this string text, IIntInterval interval)
        {
            return text.Substring(interval.Start, interval.End - interval.Start + 1);
        }
    }
}