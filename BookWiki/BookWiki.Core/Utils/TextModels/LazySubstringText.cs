using BookWiki.Core.Utils.TextModels;

namespace BookWiki.Core
{
    public class LazySubstringText : ITextRange
    {
        private readonly IProperty<IText> _textProperty;

        public LazySubstringText(IProperty<IText> textProperty, int offset, int length)
        {
            _textProperty = textProperty;
            Offset = offset;
            Length = length;
        }

        public ITextRange Substring(int offset, int length)
        {
            return _textProperty.Value.Substring(Offset, Length).Substring(offset, length);
        }

        public int Length { get; }

        public string PlainText => _textProperty.Value.Substring(Offset, Length).PlainText;

        public int Offset { get; }
    }
}