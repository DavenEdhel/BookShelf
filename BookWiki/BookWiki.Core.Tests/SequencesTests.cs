using System;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace BookWiki.Core.Tests
{
    public class SequencesTests
    {
        [Test]
        public void Test()
        {
            var i = 0;

            var s = new EnumerableSequence<int>(new []{0, 1, 2, 3}.Select(x => i++), 4);
            var cached = new RunOnceSequence<int>(s);

            var two = cached.Take(2).ToArray();
            var all = cached.ToArray();
            var all2 = cached.ToArray();

            var result = s.ToArray();
            var result2 = s.ToArray();
        }
    }
}