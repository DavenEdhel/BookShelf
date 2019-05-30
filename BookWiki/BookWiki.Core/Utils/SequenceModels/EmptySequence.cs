using System.Collections;
using System.Collections.Generic;

namespace BookWiki.Core
{
    public class EmptySequence<T> : ISequence<T>
    {
        public EmptySequence()
        {
            Completion = new Progress(1);
            Completion.MarkCompleted();
        }

        public IEnumerator<T> GetEnumerator()
        {
            yield break;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Progress Completion { get; }
    }
}