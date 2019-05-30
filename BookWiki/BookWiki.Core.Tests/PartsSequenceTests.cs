using System;
using System.Linq;
using BookWiki.Core.Files.PathModels;
using NUnit.Framework;

namespace BookWiki.Core.Tests
{
    [TestFixture(typeof(StringPathPartsSequence))]
    [TestFixture(typeof(PartsSequence))]
    public class PartsSequenceTests
    {
        private IPartsSequence _sequence;
        private string _path = @"root\folder 1\innter #1";

        public PartsSequenceTests(Type t)
        {
            if (t == typeof(StringPathPartsSequence))
            {
                _sequence = new StringPathPartsSequence(_path);
            }

            if (t == typeof(PartsSequence))
            {
                _sequence = new PartsSequence(new RunOnceSequence<ITextRange>(new StringPathPartsSequence(_path)));
            }
        }

        [Test]
        public void ParseInRightTest()
        {
            var parts = _sequence.ToArray();

            Assert.AreEqual(3, parts.Length);
        }
    }
}