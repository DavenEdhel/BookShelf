using BookWiki.Core.Utils.TextModels;

namespace BookWiki.Core
{
    public static class StringExtensions
    {
        public static string Substring(this string text, IIntInterval interval)
        {
            return text.Substring(interval.Start, interval.End - interval.Start + 1);
        }

        public static string CapitalizeFirstLetter(this string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            return text[0].ToString().ToUpper() + text.Substring(1, text.Length - 1);
        }
    }
}