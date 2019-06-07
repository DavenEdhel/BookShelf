using BookWiki.Core.Utils.PropertyModels;
using BookWiki.Core.Utils.TextModels;

namespace BookWiki.Core.Findings
{
    public class NormalizedFinding : IFinding
    {
        private readonly IText _text;
        private readonly IRange _range;
        private readonly int _findingIndex;
        private readonly int _findingLength;
        private readonly CachedValue<ITextRange> _context;
        private readonly CachedValue<ITextRange> _result;

        public NormalizedFinding(ITextRange textRange, int findingIndex, int findingLength)
        {
            _text = textRange;
            _range = textRange;
            _findingIndex = findingIndex;
            _findingLength = findingLength;

            _context = new CachedValue<ITextRange>(() => _text.Substring(0, _text.Length));
            _result = new CachedValue<ITextRange>(() => _text.Substring(_findingIndex - _range.Offset, _findingLength));
        }

        public ITextRange Result => _result.Value;

        public ITextRange Context => _context.Value;

        public IFinding Normalize()
        {
            return this;
        }
    }
}