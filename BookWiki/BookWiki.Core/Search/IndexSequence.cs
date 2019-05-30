using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BookWiki.Core
{
    public class IndexSequence : ISequence<int>
    {
        private readonly string _text;
        private readonly char[] _stops;

        public IndexSequence(string text, params char[] stops)
        {
            _text = text;
            _stops = stops;
            Completion = new Progress(_text.Length);
        }

        public IEnumerator<int> GetEnumerator()
        {
            var index = 0;

            yield return index;

            var found = false;

            for (int i = 0; i < _text.Length; i++)
            {
                if (found)
                {
                    var c = _text[i];

                    if (char.IsLetterOrDigit(c))
                    {
                        found = false;

                        yield return i;
                    }
                }
                else
                {
                    var c = _text[i];

                    if (_stops.Contains(c))
                    {
                        found = true;
                    }
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