using BookWiki.Core.Utils.TextModels;
using PureOop;

namespace BookWiki.Core.Utils
{
    public class AtLeastSingleSpaceString : IText
    {
        private readonly IText _text;
        private readonly string _plainText;

        [JustOnce]
        private string SourceText => _plainText ?? _text.PlainText;

        public ITextRange Substring(int offset, int length)
        {
            if (_text != null)
            {
                return _text.Substring(offset, length);
            }

            return new SubstringText(PlainText, offset, length);
        }

        [JustOnce]
        public int Length => PlainText.Length;

        [JustOnce]
        public string PlainText
        {
            get
            {
                if (string.IsNullOrWhiteSpace(SourceText))
                {
                    return " ";
                }

                return SourceText;
            }
        }

        public AtLeastSingleSpaceString(string plainText)
        {
            _plainText = plainText;
        }

        public AtLeastSingleSpaceString(IText text)
        {
            _text = text;
        }
    }
}