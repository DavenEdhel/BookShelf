using System.Linq;
using BookWiki.Core.Files.PathModels;
using NUnit.Framework;

namespace BookWiki.Core.Tests
{
    public class PathTests
    {
        private string _path = @"root\folder 1\innter #1";

        [Test]
        public void Smoke()
        {
            var path = new FolderPath(_path);

            var last = path.Parts.Last().PlainText;

            Assert.AreEqual("innter #1", last);
        }
    }
}