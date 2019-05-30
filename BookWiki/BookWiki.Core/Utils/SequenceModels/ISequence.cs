using System.Collections.Generic;

namespace BookWiki.Core
{
    public interface ISequence<T> : IEnumerable<T>
    {
        Progress Completion { get; }
    }
}