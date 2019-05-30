using BookWiki.Core.Utils;
using NUnit.Framework;

namespace BookWiki.Core.Tests
{
    public class SpaceSeparatedRangeTest
    {
        [Test]
        public void Smoke()
        {
            var txt = @"1. Не тыркается стрелочка вниз в файловой системе.
2.Не работает ентер при создании нового рассказа.
3.Нужно, чтобы когда добавляешь файл, папки ";

            var obj = new SpaceSeparatedRange(txt, 97, 100);

            var result = txt.Substring(obj.StartIndex, obj.Length);
        }
    }
}