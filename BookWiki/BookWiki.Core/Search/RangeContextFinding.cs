using BookWiki.Core.Search;
using BookWiki.Core.Utils;

namespace BookWiki.Core.Findings
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
            return _text.Substring(_findingIndex, new Number(_findingLength, 0, _findingLength - _findingIndex).Value);
        }

        protected override ITextRange CalculateContext()
        {
            return _text.Substring(new Number(_findingIndex - _contextSize, 0, _text.Length).Value, new Number(_findingLength + _contextSize * 2, 0, _text.Length).Value);
        }
    }
}