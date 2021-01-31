using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BookWiki.Core
{
    public class IndexesOfCharacters : IEnumerable<int>
    {
        private readonly string _text;
        private readonly char[] _stops;

        public IndexesOfCharacters(string text, params char[] stops)
        {
            _text = text;
            _stops = stops;
        }

        public IEnumerator<int> GetEnumerator()
        {
            for (int i = 0; i < _text.Length; i++)
            {
                var c = _text[i];

                if (_stops.Contains(c))
                {
                    yield return i;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}