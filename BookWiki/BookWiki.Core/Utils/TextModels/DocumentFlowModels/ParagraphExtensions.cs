using System.Text;

namespace BookWiki.Core
{
    public static class ParagraphExtensions
    {
        public static string GetAllText(this IParagraph p)
        {
            var sb = new StringBuilder();

            foreach (var pInline in p.Inlines)
            {
                sb.Append(pInline.Text.PlainText);
            }

            return sb.ToString();
        }
    }
}