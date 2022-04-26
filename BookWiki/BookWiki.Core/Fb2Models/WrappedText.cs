using BookWiki.Core.Utils.TextModels;

namespace BookWiki.Core.Fb2Models
{
    public class WrappedText : IString
    {
        public WrappedText(string text, string tag)
        {
            Tag = tag;
            Text = text;
        }

        public string Tag { get; } = "empty";

        public string Text { get; } = string.Empty;

        public string Value => $"<{Tag}>{Text}</{Tag}>";
    }
}