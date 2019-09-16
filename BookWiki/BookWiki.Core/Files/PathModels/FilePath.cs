using System.IO;
using BookWiki.Core.Utils.PropertyModels;

namespace BookWiki.Core.Files.PathModels
{
    public class FilePath : IAbsolutePath
    {
        private readonly IProperty<string> _path;

        private readonly IPath _folderPath;

        public FilePath(IPath path, string nextLevelName)
        {
            _folderPath = path;
            _path = new CachedValue<string>(() => Path.Combine(path.FullPath, nextLevelName));
            Parts = new StringPathPartsSequence(_path);
        }

        public IFileName Name => _folderPath.Name;

        public IExtension Extension => _folderPath.Extension;

        public string FullPath => _path.Value;

        public bool EqualsTo(IPath path)
        {
            return _path.Value.Equals(path.FullPath);
        }

        public IPartsSequence Parts { get; }
    }
}