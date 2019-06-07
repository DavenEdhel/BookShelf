using System.Collections.Generic;
using System.Linq;
using BookWiki.Core.Content;
using NUnit.Framework;

namespace BookWiki.Core.Tests
{
    public class ArticleTests
    {
        private FakeArticle _article;

        [SetUp]
        public void Setup()
        {
            _article = new FakeArticle(new List<ArticlePart>()
            {
                ArticlePart.CreateHeader("Some new article\n"),
                ArticlePart.CreateText("Blah blah text with "),
                ArticlePart.CreateLink("link to another", "Another Article"),
                ArticlePart.CreateText(" article.\n"),
                ArticlePart.CreateText("Next P.")
            });
        }

        [Test]
        public void InitializationTest()
        {
            var expected = "Some new article\nBlah blah text with link to another article.\nNext P.";

            var result = _article.Content.PlainText;

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void InsertTextTest()
        {
            var expected = "Some new ar123ticle\nBlah blah text with link to another article.\nNext P.";

            _article.InsertPart(11, "123");

            var result = _article.Content.PlainText;

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void InsertEnterInHeaderTest()
        {
            var expected = "Some new ar\nticle\nBlah blah text with link to another article.\nNext P.";

            _article.InsertPart(11, "\n");

            var result = _article.Content.PlainText;

            Assert.AreEqual(expected, result);

            var parts = _article.ToArticleParts().ToArray();

            Assert.AreEqual(MarkerStyle.Header, parts.ElementAt(0).MarkerStyle);
            Assert.AreEqual("Some new ar\n", parts.ElementAt(0).Text);

            Assert.AreEqual(MarkerStyle.Text, parts.ElementAt(1).MarkerStyle);
            Assert.AreEqual("ticle\n", parts.ElementAt(1).Text);
        }

        [Test, Ignore("Obsolete")]
        public void InsertToEmptyTest()
        {
            _article = new FakeArticle(new List<ArticlePart>());

            var expected = "Some";

            _article.InsertPart(0, "Some");

            var result = _article.Content.PlainText;

            Assert.AreEqual(expected, result);

            _article.InsertPart(4, "\n");

            Assert.AreEqual("Some\n", _article.Content.PlainText);

            _article.InsertPart(5, "new");

            Assert.AreEqual("Some\nnew", _article.Content.PlainText);
        }

        [Test]
        public void InsertTextInsideLinkTest()
        {
            var expected = "Some new article\nBlah blah text with link 123 to another article.\nNext P.";

            _article.InsertPart(41, " 123");

            var result = _article.Content.PlainText;

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void RemoveSmallPartTest()
        {
            var expected = "Some  article\nBlah blah text with link to another article.\nNext P.";

            _article.RemovePart(5, 3);

            var result = _article.Content.PlainText;

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void RemoveBigPartTest()
        {
            var expected = "Some new blah text with link to another article.\nNext P.";

            _article.RemovePart(8, 13);

            var result = _article.Content.PlainText;

            Assert.AreEqual(expected, result);
        }
    }
}