using BookWiki.Core.Utils.TextModels;

namespace BookWiki.Core
{
    public class MutableTextInfo : ITextInfo
    {
        private readonly MutableRange _mutableRange;

        public MutableTextInfo(ITextInfo textInfo)
        {
            _mutableRange = new MutableRange()
            {
                Offset = textInfo.Range.Offset,
                Length = textInfo.Range.Length
            };

            Style = textInfo.Style;
        }

        public MutableRange MutableRange => _mutableRange;

        public IRange Range => MutableRange;

        public TextStyle Style { get; set; }
    }
}