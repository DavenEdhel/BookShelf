using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BookWiki.Core.Utils.PropertyModels;

namespace BookWiki.Core.Search
{
    public class TextRangeSequence : ISequence<ITextRange>
    {
        private readonly IProperty<IText> _text;
        private readonly char[] _stops;

        public TextRangeSequence(string path, params char[] stops)
        {
            _text = new CachedValue<IText>(() => new StringText(path));
            Completion = new Progress(new CachedValue<int>(path.Length));
            _stops = stops;
        }

        public TextRangeSequence(IText text, params char[] stops)
        {
            _text = new CachedValue<IText>(() => text);
            _stops = stops;
            Completion = new Progress(new CachedValue<int>(() => _text.Value.PlainText.Length));
        }

        public TextRangeSequence(IProperty<IText> text, IProperty<int> length, params char[] stops)
        {
            _text = text;
            _stops = stops;
            Completion = new Progress(length);
        }

        public IEnumerator<ITextRange> GetEnumerator()
        {
            var index = 0;

            var plainText = _text.Value.PlainText;

            for (int i = 0; i < _text.Value.Length; i++)
            {
                var c = plainText[i];

                if (_stops.Contains(c))
                {
                    yield return _text.Value.Substring(index, i - index);

                    index = i + 1;
                }

                Completion.Increment();
            }

            yield return _text.Value.Substring(index, plainText.Length - index);

            Completion.MarkCompleted();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Progress Completion { get; }
    }
}