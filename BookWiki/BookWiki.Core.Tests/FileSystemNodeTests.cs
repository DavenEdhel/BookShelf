using System.Linq;
using BookWiki.Core.Files.FileSystemModels;
using NUnit.Framework;

namespace BookWiki.Core.Tests
{
    public class FileSystemNodeTests
    {
        [Test]
        public void EqualityTest()
        {
            var root = FakeFileSystemNode.InitFakeAndReturnRoot();

            var a1 = root.InnerNodes.First();
            var b1 = root.InnerNodes.Last();
            var a2 = a1.InnerNodes.First();

            var comparer = new FileSystemNodeEquality(a1, a2);
            var state = comparer.EqualityState;

            Assert.AreEqual(Equality.FirstLower, state);

            var comparer2 = new FileSystemNodeEquality(a2, b1);
            state = comparer2.EqualityState;

            Assert.AreEqual(Equality.FirstLower, state);
        }
    }
}