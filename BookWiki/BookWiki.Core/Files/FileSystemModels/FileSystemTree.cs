using System.Collections.Generic;
using System.Linq;
using BookWiki.Core.Files.PathModels;
using Keurig.IQ.Core.CrossCutting.Extensions;

namespace BookWiki.Core.Files.FileSystemModels
{
    public class FileSystemTree : IFileSystemTree
    {
        private readonly ISequence<IPath> _allItems;

        public ISequence<IPath> RootPaths { get; }

        public FileSystemTree(IFileSystem fileSystem)
        {
            _allItems = fileSystem.Contents;

            RootPaths = new RunOnceSequence<IPath>(new EnumerableSequence<IPath>(GetRootPaths()));
        }

        private IEnumerable<IPath> GetRootPaths()
        {
            var firstLayerPaths = _allItems.Distinct(new EqualsByFirstPartStrategy());

            return firstLayerPaths.OrderBy(x => x.Parts.First().PlainText);
        }

        class EqualsByFirstPartStrategy : IEqualityComparer<IPath>
        {
            public bool Equals(IPath x, IPath y)
            {
                var f1 = x.Parts.First();
                var f2 = y.Parts.First();

                return f1.PlainText == f2.PlainText && f1.Offset == f2.Offset && f1.CastTo<IText>().Length == f2.CastTo<IText>().Length;
            }

            public int GetHashCode(IPath obj)
            {
                return obj.GetHashCode();
            }
        }
    }
}