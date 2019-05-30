using System;
using System.Collections;
using System.Collections.Generic;

namespace BookWiki.Core
{
    public class EnumerableSequence<T> : ISequence<T>
    {
        private readonly IEnumerable<T> _source;

        public EnumerableSequence(IEnumerable<T> source, int length)
        {
            _source = source;
            Completion = new Progress(length);
        }

        public EnumerableSequence(IEnumerable<T> source) : this(source, 100)
        {
        }

        public Progress Completion { get; }

        public IEnumerator<T> GetEnumerator()
        {
            using (var enumerator = _source.GetEnumerator())
            {
                Completion.Change(0);

                while (enumerator.MoveNext())
                {
                    yield return enumerator.Current;

                    Completion.Increment();
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}