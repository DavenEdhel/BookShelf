using System;
using System.Linq;
using BookWiki.Core.Utils.TextModels;
using Keurig.IQ.Core.CrossCutting.Extensions;

namespace BookWiki.Core.Utils
{
    public class PunctuationSeparatedRange : ITextRange
    {
        private readonly string _plainText;
        private readonly int _startIndex;
        private readonly int _endIndex;

        public PunctuationSeparatedRange(string plainText, int startIndex = 0, int length = -1)
        {
            var punctuation = new Char[] {' ', '.', ',', '!', '?', ';', ':', '\n'};

            _plainText = plainText;
            _startIndex = new Number(startIndex, 0, _plainText.Length - 1);
            _endIndex = new Number(length == -1 ? _plainText.Length : startIndex + length, 0, _plainText.Length);

            if (_startIndex != 0)
            {
                var offset = _plainText.Skip(_startIndex).TakeWhile(c => c.IsAnyOf(punctuation) == false).Count();

                _startIndex += offset;
            }

            {
                var sub = _plainText.Substring(_startIndex, _endIndex - _startIndex);

                var offset = sub.Reverse().TakeWhile(c => c.IsAnyOf(punctuation) == false).Count();

                _endIndex -= offset;
            }
        }

        public ITextRange Substring(int offset, int length)
        {
            return new SubstringText(PlainText, offset, length);
        }

        public int Length => _endIndex - _startIndex;

        public string PlainText => _plainText.Substring(_startIndex, Length);

        public int Offset => _startIndex;
    }
}