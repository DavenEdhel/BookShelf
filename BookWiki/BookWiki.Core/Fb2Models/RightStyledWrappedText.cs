using BookWiki.Core.Utils.TextModels;

namespace BookWiki.Core.Fb2Models
{
    public class RightStyledWrappedText : IString
    {
        public RightStyledWrappedText(string text, string tag)
        {
            _tag = tag;
            _text = text;
        }

        private readonly string _tag;
        private readonly string _text;

        public string Value => $"<{_tag} style=\"text-align: right;\">{_text}</{_tag}>\n";
    }
}