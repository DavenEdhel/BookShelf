using BookWiki.Core.Utils;
using BookWiki.Core.Utils.PropertyModels;
using BookWiki.Core.Utils.TextModels;

namespace BookWiki.Core
{
    public class SubstringText : ITextRange
    {
        private readonly string _originContent;

        public int Length { get; }

        public int Offset { get; }

        private int OuterGlobalOffset => _outerSubstring?.GlobalOffset ?? 0;

        private int OuterLength => _outerSubstring?.Length ?? _originContent.Length;

        private Number GlobalOffset => new Number(OuterGlobalOffset + Offset, minimum: 0, maximum: _originContent.Length);

        private readonly SubstringText _outerSubstring;

        private readonly CachedValue<string> _content;

        public SubstringText(string originContent) : this(originContent, 0, originContent.Length)
        {
        }

        public SubstringText(string originContent, int offset, int length) : this (originContent, offset, length, null)
        {
        }

        public SubstringText(string originContent, int offset, int length, SubstringText outerSubstring)
        {
            _originContent = originContent;
            Offset = offset;
            Length = length;
            _outerSubstring = outerSubstring;

            _content = new CachedValue<string>(() =>
            {
                var normalizedLength = new Number(Length, 0, OuterGlobalOffset + OuterLength - GlobalOffset);

                return _originContent.Substring(GlobalOffset, normalizedLength);
            });
        }

        public ITextRange Substring(int offset, int length)
        {
            return new SubstringText(_originContent, offset, length, this);
        }

        public string PlainText => _content.Value;
    }
}