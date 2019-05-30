using BookWiki.Core.Utils;
using NUnit.Framework;

namespace BookWiki.Core.Tests
{
    public class SpaceSeparatedRangeTest
    {
        [Test]
        public void Smoke()
        {
            var txt = @"1. �� ��������� ��������� ���� � �������� �������.
2.�� �������� ����� ��� �������� ������ ��������.
3.�����, ����� ����� ���������� ����, ����� ";

            var obj = new SpaceSeparatedRange(txt, 97, 100);

            var result = txt.Substring(obj.StartIndex, obj.Length);
        }
    }
}