using System.Linq;
using BookWiki.Core.Fb2Models;
using BookWiki.Core.Files.PathModels;
using Keurig.Tests.Common.Utils;
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

    public class WrapEachChapterInSectionTests
    {
        [Test]
        public void SplitByWord()
        {
            var result = new SplitByWord("<t>1</t>something<t>2</t>some2", "<t>").ToArray();
            result[0].ClaimEqual("<t>1</t>something");
            result[1].ClaimEqual("<t>2</t>some2");
        }

        [Test]
        public void SplitByTag()
        {
            var result = new SplitByTag("<t>1</t>something<t>2</t>some2", "t").ToArray();
            result[0].ClaimEqual("<t>1</t>something");
            result[1].ClaimEqual("<t>2</t>some2");
        }

        [Test]
        public void CrapBefore()
        {
            var result = new SplitByTag("<br /><title><p>Глава 5. Вера в успех</p></title>", "title").ToArray();
            result[0].ClaimEqual("<title><p>Глава 5. Вера в успех</p></title>");
        }
    }
}