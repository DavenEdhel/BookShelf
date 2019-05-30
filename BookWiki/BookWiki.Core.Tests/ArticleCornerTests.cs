using BookWiki.Core.Content;
using NUnit.Framework;

namespace BookWiki.Core.Tests
{
    public class ArticleCornerTests
    {
        private FakeArticle _article;

        [Test]
        public void Case001()
        {
            _article = new FakeArticle(Articles.Case001);

            var expected = "Some new article\r\nBlah blah text with link to another article.\r\nNext P. ";

            _article.InsertPart(71, " ");

            var result = _article.Content.PlainText;

            Assert.AreEqual(expected, result);
        }
    }
}