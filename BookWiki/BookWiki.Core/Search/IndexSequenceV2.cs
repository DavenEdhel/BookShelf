using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BookWiki.Core
{
    public class IndexSequenceV2 : ISequence<int>
    {
        private readonly string _text;
        private readonly char[] _stops;

        public IndexSequenceV2(string text, params char[] stops)
        {
            _text = text;
            _stops = stops;
            Completion = new Progress(_text.Length);
        }

        public IEnumerator<int> GetEnumerator()
        {
            for (int i = 0; i < _text.Length; i++)
            {
                var c = _text[i];

                if (_stops.Contains(c))
                {
                    yield return i + 1;
                }

                Completion.Increment();
            }

            Completion.MarkCompleted();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Progress Completion { get; }
    }
}