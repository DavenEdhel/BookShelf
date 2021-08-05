using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BookWiki.Core.Utils.TextModels;

namespace BookWiki.Core
{
    public class IntervalsBetweenCharacters : IEnumerable<IIntInterval>
    {
        private readonly string _text;
        private readonly char[] _stops;

        public IntervalsBetweenCharacters(string text, params char[] stops)
        {
            _text = text;
            _stops = stops;
        }

        public IEnumerator<IIntInterval> GetEnumerator()
        {
            var indexesOfStops = new List<int>() { -1 };
            indexesOfStops.AddRange(new IndexesOfCharacters(_text, _stops).ToArray());
            indexesOfStops.Add(_text.Length);

            for (int i = 0; i < indexesOfStops.Count - 1; i++)
            {
                if (indexesOfStops[i + 1] - indexesOfStops[i] > 1)
                {
                    yield return new IntInterval()
                    {
                        Start = indexesOfStops[i] + 1,
                        End = indexesOfStops[i + 1] - 1,
                    };
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}