using BookWiki.Core.Utils.PropertyModels;

namespace BookWiki.Core.Findings
{
    public class NormalizedFinding : IFinding
    {
        private readonly ITextRange _text;
        private readonly int _findingIndex;
        private readonly int _findingLength;
        private readonly CachedValue<ITextRange> _context;
        private readonly CachedValue<ITextRange> _result;

        public NormalizedFinding(ITextRange text, int findingIndex, int findingLength)
        {
            _text = text;
            _findingIndex = findingIndex;
            _findingLength = findingLength;

            _context = new CachedValue<ITextRange>(() => _text.Substring(0, _text.Length));
            _result = new CachedValue<ITextRange>(() => _text.Substring(_findingIndex - _text.Offset, _findingLength));
        }

        public ITextRange Result => _result.Value;

        public ITextRange Context => _context.Value;

        public IFinding Normalize()
        {
            return this;
        }
    }
}