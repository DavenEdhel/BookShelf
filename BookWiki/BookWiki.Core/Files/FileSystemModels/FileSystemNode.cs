using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BookWiki.Core.Files.PathModels;
using BookWiki.Core.Utils.PropertyModels;
using PureOop;

namespace BookWiki.Core.Files.FileSystemModels
{
    public class ChildFileSystemNodeCollection : IEnumerable<IFileSystemNode>
    {
        private readonly IFileSystemNode _root;

        private readonly List<IFileSystemNode> _all = new List<IFileSystemNode>();

        public ChildFileSystemNodeCollection(IFileSystemNode root)
        {
            _root = root;
        }

        public IEnumerator<IFileSystemNode> GetEnumerator()
        {
            if (_all.Count > 0)
            {
                return _all.GetEnumerator();
            }

            _all.Add(_root);

            Process(_root);

            return _all.GetEnumerator();
        }

        private void Process(IFileSystemNode root)
        {
            _all.AddRange(root.InnerNodes);

            foreach (var fileSystemNode in root.InnerNodes)
            {
                Process(fileSystemNode);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class FileSystemNode : IFileSystemNode
    {
        private readonly string _path;

        private readonly IProperty<EnumerableSequence<IFileSystemNode>> _innerNodes;

        public FileSystemNode(string root) : this(root, 0)
        {
        }

        public FileSystemNode(IFileSystemNode parent, IPath path) : this(path.FullPath, parent.Level + 1)
        {
        }

        public FileSystemNode(string path, int level)
        {
            _path = path;
            Level = level;

            _innerNodes = new CachedValue<EnumerableSequence<IFileSystemNode>>(() => new EnumerableSequence<IFileSystemNode>(Directory.GetDirectories(_path).Select(x => new FileSystemNode(x, Level + 1))));
            Path = new FolderPath(path);
        }

        public ISequence<IFileSystemNode> InnerNodes => _innerNodes.Value;

        public int Level { get; }

        //[JustOnce]
        public bool IsContentFolder => Path.Extension.Type != NodeType.Directory;

        public void SaveUnder(IFileSystemNode parent)
        {
            Directory.CreateDirectory(Path.FullPath);
        }

        public IPath Path { get; } 
    }
}