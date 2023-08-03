using System.Linq;
using BookWiki.Core.Utils.TextModels;

namespace BookWiki.Core.Files.PathModels
{
    public class DirectoryPath : IPath
    {
        public DirectoryPath(IPath filePath)
        {
            Parts = new PartsSequence(new EnumerableSequence<ITextRange>(filePath.Parts.Reverse().Skip(1).Reverse()));
        }

        public IFileName Name => new FileName(Parts);
        public IExtension Extension => new Extension(Parts);
        public string FullPath => Parts.FullPath;

        public bool EqualsTo(IPath path)
        {
            return FullPath == path.FullPath;
        }

        public IPartsSequence Parts { get; }
    }

    public class AbsoluteDirectoryPath : IAbsolutePath
    {
        private readonly DirectoryPath _absoluteDirPath;

        public AbsoluteDirectoryPath(IRootPath root, IRelativePath pathToContent)
        {
            _absoluteDirPath = new DirectoryPath(pathToContent.AbsolutePath(root));
        }

        public IFileName Name => _absoluteDirPath.Name;
        public IExtension Extension => _absoluteDirPath.Extension;
        public string FullPath => _absoluteDirPath.FullPath;
        public bool EqualsTo(IPath path)
        {
            return _absoluteDirPath.EqualsTo(path);
        }

        public IPartsSequence Parts => _absoluteDirPath.Parts;
    }
}