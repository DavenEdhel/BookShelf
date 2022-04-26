using BookWiki.Core.Utils.TextModels;

namespace BookWiki.Core.Fb2Models
{
    public class Fb2Paragraph : IString
    {
        public Fb2Paragraph(string content, ITextInfo format)
        {
            if (format != null)
            {
                switch (format.Style)
                {
                    case TextStyle.Centered:
                        Value = new WrappedText(new WrappedText(content, "p").Value, "title").Value;
                        break;

                    case TextStyle.Right:
                        Value = new WrappedText(new WrappedText(content, "emphasis").Value, "p").Value;
                        break;

                    default:
                        Value = content;
                        break;
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(content))
                {
                    Value = "<br />";
                }
                else
                {
                    Value = new WrappedText(content, "p").Value;
                }
            }
        }

        public string Value { get; }
    }
}