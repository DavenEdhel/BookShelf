using BookWiki.Core.Findings;
using NUnit.Framework;

namespace BookWiki.Core.Tests
{
    public class FindingsTests
    {
        [Test]
        public void ContextRangeFindingTest()
        {
            var text = new StringText("0123456789");
            var finding = new RangeContextFinding(text, 3, 5, contextSize: 1);

            var marker1 = finding.Result.PlainText;
            Assert.AreEqual(3, finding.Result.Offset);
            Assert.AreEqual("34567", marker1);

            var content1 = finding.Context.PlainText;
            Assert.AreEqual("2345678", content1);

            var normalized = finding.Normalize();

            var marker2 = normalized.Result.PlainText;
            Assert.AreEqual(1, normalized.Result.Offset);
            Assert.AreEqual("34567", marker2);

            var content2 = normalized.Context.PlainText;
            Assert.AreEqual("2345678", content2);
        }
    }
}