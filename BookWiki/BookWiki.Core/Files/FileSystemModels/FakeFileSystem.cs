using BookWiki.Core.Files.FileSystemModels;
using BookWiki.Core.Files.PathModels;
using BookWiki.Core.FileSystem.PathModels;

namespace BookWiki.Core
{
    public class FakeFileSystem : IFileSystem
    {
        public ISequence<IPath> Contents { get; }

        public FakeFileSystem()
        {
            Contents = new RunOnceSequence<IPath>(new ArraySequence<IPath>(new[]
            {
                new FakePath("1"),
                new FakePath("2"),
                new FakePath("3"),
                new FakePath("4"),
                new FakePath("5"),
            }));
        }
    }
}