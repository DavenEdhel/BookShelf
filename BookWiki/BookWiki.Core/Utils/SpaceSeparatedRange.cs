using System.Linq;

namespace BookWiki.Core.Utils
{
    public class SpaceSeparatedRange
    {
        private string _plainText;
        private int _startIndex;
        private int _endIndex;

        public SpaceSeparatedRange(string plainText, int startIndex = 0, int length = -1)
        {
            _plainText = plainText;
            _startIndex = new Number(startIndex, 0, _plainText.Length - 1).Value;
            _endIndex = new Number(length == -1 ? plainText.Length : startIndex + length, 0, plainText.Length - 1).Value;

            if (_startIndex != 0)
            {
                var offset = _plainText.Skip(_startIndex).TakeWhile(c => c != ' ').Count();

                _startIndex += offset;
            }

            if (_endIndex != _plainText.Length)
            {
                var sub = _plainText.Substring(_startIndex, _endIndex - _startIndex);

                var offset = sub.Reverse().TakeWhile(c => c != ' ').Count();

                _endIndex -= offset;
            }
        }

        public int StartIndex => _startIndex;

        public int EndIndex => _endIndex;

        public int Length => _endIndex - _startIndex;
    }
}