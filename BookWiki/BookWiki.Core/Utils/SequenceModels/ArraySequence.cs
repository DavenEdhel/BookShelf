using System.Collections;
using System.Collections.Generic;

namespace BookWiki.Core
{
    public class ArraySequence<T> : ISequence<T>
    {
        private readonly T[] _items;

        public ArraySequence(T[] items)
        {
            _items = items;

            Completion = new Progress(_items.Length);
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (var item in _items)
            {
                yield return item;

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