using System.Linq;
using BookWiki.Core.Utils.PropertyModels;

namespace BookWiki.Core.Files.PathModels
{
    public class TextRangeFileName : IFileName
    {
        private readonly ITextRange _textRange;

        public TextRangeFileName(ITextRange textRange)
        {
            _textRange = textRange;
            _name = new CachedValue<ITextRange>(() => { return _textRange.SplitBy('.').First(); });
        }

        public string PlainText => _name.Value.PlainText;

        private readonly IProperty<ITextRange> _name;
    }
}