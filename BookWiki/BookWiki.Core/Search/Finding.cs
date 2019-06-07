using BookWiki.Core.Findings;
using BookWiki.Core.Utils.PropertyModels;
using BookWiki.Core.Utils.TextModels;

namespace BookWiki.Core.Search
{
    public abstract class Finding : IFinding
    {
        private readonly int _findingIndex;
        private readonly int _findingLength;

        private readonly CachedValue<IFinding> _normalize;
        private readonly CachedValue<ITextRange> _context;
        private readonly CachedValue<ITextRange> _result;

        protected Finding(int findingIndex, int findingLength)
        {
            _findingIndex = findingIndex;
            _findingLength = findingLength;

            _normalize = new CachedValue<IFinding>(() => new NormalizedFinding(Context, _findingIndex, _findingLength));
            _context = new CachedValue<ITextRange>(CalculateContext);
            _result = new CachedValue<ITextRange>(CalculateResult);
        }

        protected abstract ITextRange CalculateResult();

        protected abstract ITextRange CalculateContext();

        public ITextRange Result => _result.Value;

        public ITextRange Context => _context.Value;

        public IFinding Normalize() => _normalize.Value;
    }
}