using BookWiki.Core.Utils;
using NUnit.Framework;

namespace BookWiki.Core.Tests
{
    public class NumberTests
    {
        [Test]
        public void InRange()
        {
            var n = new Number(5, 0, 10);

            Assert.AreEqual(5, (int)n);
        }

        [Test]
        public void InStringRange()
        {
            var n = new Number(3, "abcde");

            Assert.AreEqual(3, (int)n);
        }

        [Test]
        public void OutOfRangeAbove()
        {
            var n = new Number(8, "abcde");

            Assert.AreEqual(4, (int)n);
        }

        [Test]
        public void OutOfRangeBelow()
        {
            var n = new Number(-3, "abcde");

            Assert.AreEqual(0, (int)n);
        }

        [Test]
        public void GreaterThanMax()
        {
            var n = new Number(8, 0, 5);

            Assert.AreEqual(5, (int)n);
        }

        [Test]
        public void LowerThanMin()
        {
            var n = new Number(-3, 0, 5);

            Assert.AreEqual(0, (int)n);
        }

        [Test]
        public void Crash001()
        {
            var length = 100;
            var plainTextLength = 29964;
            var startIndex = 29914;

            // [30014 in (0, 29963)] = 22963

            var number = new Number(length == -1 ? plainTextLength : (startIndex + length), 0, plainTextLength - 1);

            Assert.AreEqual(29963, (int)number);
        }
    }
}