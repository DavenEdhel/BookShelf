using System.IO;
using System.Linq;
using BookWiki.Core.Files.PathModels;
using BookWiki.Core.Utils.PropertyModels;
using PureOop;

namespace BookWiki.Core.Files.FileSystemModels
{
    public class FileSystemNode : IFileSystemNode
    {
        private readonly string _path;

        private readonly IProperty<EnumerableSequence<IFileSystemNode>> _innerNodes;

        public FileSystemNode(string root) : this(root, 0)
        {
        }

        public FileSystemNode(IFileSystemNode parent, IAbsolutePath path) : this(path.FullPath, parent.Level + 1)
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

        public IAbsolutePath Path { get; } 
    }
}