using BookWiki.Core.Utils;
using BookWiki.Core.Utils.TextModels;

namespace BookWiki.Core.Search
{
    public class RangeContextFinding : Finding
    {
        private readonly IText _text;
        private readonly int _findingIndex;
        private readonly int _findingLength;
        private readonly int _contextSize;

        public RangeContextFinding(IText text, int findingIndex, int findingLength, int? contextSize = null) : base(findingIndex, findingLength)
        {
            _text = text;
            _findingIndex = findingIndex;
            _findingLength = findingLength;
            _contextSize = contextSize ?? 100;
        }

        public RangeContextFinding(IContent article) : this(article.Content, 0, article.Title.Length)
        {
        }

        protected override ITextRange CalculateResult()
        {
            var maxLength = new Number(_findingLength + _findingIndex, 0, _text.Length);

            return _text.Substring(_findingIndex, new Number(_findingLength, 0, maxLength));
        }

        protected override ITextRange CalculateContext()
        {
            return _text.Substring(new Number(_findingIndex - _contextSize, 0, _text.Length), new Number(_findingLength + _contextSize * 2, 0, _text.Length));
        }
    }
}