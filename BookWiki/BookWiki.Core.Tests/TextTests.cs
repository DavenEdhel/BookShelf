using NUnit.Framework;

namespace BookWiki.Core.Tests
{
    public class TextTests
    {
        [Test]
        public void SmokeTest()
        {
            var text = new StringText("0123456789");

            var s1 = text.Substring(3, 5);
            Assert.AreEqual("34567", s1.PlainText);

            var s2 = s1.Substring(2, 3);
            Assert.AreEqual("567", s2.PlainText);
        }

        [Test]
        public void OutOfRangeTest()
        {
            var text = new StringText("0123456789");

            var s1 = text.Substring(3, 5);

            Assert.AreEqual("34567", s1.PlainText);

            var s2 = s1.Substring(2, 4);

            Assert.AreEqual("567", s2.PlainText);
        }
    }
}