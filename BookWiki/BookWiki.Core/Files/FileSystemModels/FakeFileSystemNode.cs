using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BookWiki.Core.Files.PathModels;
using Keurig.IQ.Core.CrossCutting.Extensions;

namespace BookWiki.Core.Files.FileSystemModels
{
    public class FakeFileSystemNode : IFileSystemNode
    {
        private List<IFileSystemNode> _innerNodes;

        public static IFileSystemNode InitFakeAndReturnRoot()
        {
            return new FakeFileSystemNode("root", false, 
                new FakeFileSystemNode("root@folder 1", false,
                    new FakeFileSystemNode("root@folder 1@innter #1", false),
                    new FakeFileSystemNode("root@folder 1@content folder", true),
                    new FakeFileSystemNode("root@folder 1@innter #2", false,
                        new FakeFileSystemNode("root@folder 1@innter #2@content #1", true),
                        new FakeFileSystemNode("root@folder 1@innter #2@content #3", true),
                        new FakeFileSystemNode("root@folder 1@innter #2@content #0", true))),
                new FakeFileSystemNode("root@folder 2", false,
                    new FakeFileSystemNode("root@folder 2@innter #1", false),
                    new FakeFileSystemNode("root@folder 2@content folder", true),
                    new FakeFileSystemNode("root@folder 2@innter #2", false,
                        new FakeFileSystemNode("root@folder 2@innter #2@content #1", true),
                        new FakeFileSystemNode("root@folder 2@innter #2@content #3", true),
                        new FakeFileSystemNode("root@folder 2@innter #2@content #0", true))));
        }

        public FakeFileSystemNode(string path, bool isContentFolder, params IFileSystemNode[] innerNodes)
        {
            path = path.Replace('@', System.IO.Path.DirectorySeparatorChar);

            _innerNodes = new List<IFileSystemNode>(innerNodes ?? new IFileSystemNode[0]);

            IsContentFolder = isContentFolder;
            Path = new FolderPath(path);
            InnerNodes = new EnumerableSequence<IFileSystemNode>(_innerNodes);
        }

        public ISequence<IFileSystemNode> InnerNodes { get; }

        public IPath Path { get; }

        public bool IsContentFolder { get; }

        public int Level => Path.Parts.Count();

        public void SaveUnder(IFileSystemNode parent)
        {
            parent.CastTo<FakeFileSystemNode>()._innerNodes.Add(this);
        }
    }
}