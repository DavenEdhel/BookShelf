using BookWiki.Core.Utils.TextModels;

namespace BookWiki.Core
{
    public class StringText : IText
    {
        private readonly string _str;

        public StringText(string str)
        {
            _str = str;
        }

        public ITextRange Substring(int offset, int length)
        {
            return new SubstringText(_str, offset, length);
        }

        public int Length => _str.Length;

        public string PlainText => _str;
    }
}