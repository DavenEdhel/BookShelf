using System;
using System.IO;
using System.Text;
using NUnit.Framework;

namespace BookWiki.Core.Tests
{
    public class TextTests
    {
        [Test]
        public void Weird()
        {
            var result = File.Exists(
                @"C:\Work\Projects\BookShelf\BookWiki\BookWiki.Presentation.Wpf\bin\Debug\Book Wiki 4\Рассказы\Вторжение\Материалы\Безымянный.n\Text.txt");

            var result1 =
                @"C:\Work\Projects\BookShelf\BookWiki\BookWiki.Presentation.Wpf\bin\Debug\Book Wiki 4\Рассказы\Вторжение\Материалы\Безымянный.n\Text.txt";
            var result2 =
                @"C:\Work\Projects\BookShelf\BookWiki\BookWiki.Presentation.Wpf\bin\Debug\Book Wiki 4\Рассказы\Вторжение\Материалы\Безымянный.n\Text.txt";

            var r1 = File.Exists(result1);
            var r2 = File.Exists(result2);

            var b = Encoding.Unicode.GetBytes(result1);

            var result3 = Encoding.UTF8.GetString(b);

            var r3 = File.Exists(result3);
        }

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