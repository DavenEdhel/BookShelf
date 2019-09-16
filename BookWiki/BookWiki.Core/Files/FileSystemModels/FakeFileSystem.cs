using BookWiki.Core.Files.FileSystemModels;
using BookWiki.Core.Files.PathModels;

namespace BookWiki.Core
{
    public class FakeFileSystem : IFileSystem
    {
        public ISequence<IAbsolutePath> Contents { get; }

        public FakeFileSystem()
        {
            Contents = new RunOnceSequence<IAbsolutePath>(new ArraySequence<IAbsolutePath>(new[]
            {
                new PathFake("1"),
                new PathFake("2"),
                new PathFake("3"),
                new PathFake("4"),
                new PathFake("5"),
            }));
        }
    }
}