using System.Collections;
using System.Collections.Generic;

namespace BookWiki.Core
{
    public class RunOnceSequence<T> : ISequence<T>
    {
        private readonly ISequence<T> _sequence;
        private readonly List<T> _cache = new List<T>();
        private readonly IEnumerator<T> _enumerator;

        public Progress Completion => _sequence.Completion;

        public RunOnceSequence(ISequence<T> sequence)
        {
            _sequence = sequence;
            _enumerator = _sequence.GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (var item in _cache.ToArray())
            {
                yield return item;
            }

            while (_enumerator.MoveNext())
            {
                var current = _enumerator.Current;

                _cache.Add(current);

                yield return current;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}