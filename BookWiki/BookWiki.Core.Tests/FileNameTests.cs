using BookWiki.Core.Files.PathModels;
using NUnit.Framework;

namespace BookWiki.Core.Tests
{
    public class FileNameTests
    {
        [Test]
        public void SimpleName()
        {
            var fileName = new FileName("Something something");

            Assert.AreEqual("Something something", fileName.PlainText);
        }

        [Test]
        public void NameWithDot()
        {
            var fileName = new FileName("Chapter 1. Something");

            Assert.AreEqual("Chapter 1. Something", fileName.PlainText);
        }

        [Test]
        public void NameWithExtension()
        {
            var fileName = new FileName("Chapter 1.n");

            Assert.AreEqual("Chapter 1", fileName.PlainText);
        }

        [Test]
        public void NameWithThreeParts()
        {
            var fileName = new FileName("Chapter 1. Something.n");

            Assert.AreEqual("Chapter 1. Something", fileName.PlainText);
        }
    }
}