using BookWiki.Core.Utils;
using NUnit.Framework;

namespace BookWiki.Core.Tests
{
    public class PunctuationSeparatedRangeTests
    {
        [Test]
        public void Smoke()
        {
            var t = "Очень простая фраза, ко";
            var expected = "Очень простая фраза, ";

            var p = new PunctuationSeparatedRange(t);

            var textRange = new StringText(t).Substring(p.Offset, p.Length);

            Assert.AreEqual(expected, textRange.PlainText);
        }

        [Test]
        public void SpaceAsLastSymbol()
        {
            var t = "Очень простая фраза, ко ";
            var expected = "Очень простая фраза, ко ";

            var p = new PunctuationSeparatedRange(t);

            var textRange = new StringText(t).Substring(p.Offset, p.Length);

            Assert.AreEqual(expected, textRange.PlainText);
        }
    }
}