using System.IO;
using BookWiki.Core.Files.PathModels;
using Newtonsoft.Json;

namespace BookWiki.Presentation.Wpf.Models
{
    public class RootPath : IRootPath
    {
        private readonly IPath _path;

        public RootPath(string path)
        {
            _path = new FolderPath(path);
        }

        public IFileName Name => _path.Name;

        public IExtension Extension => _path.Extension;

        public string FullPath => _path.FullPath;

        public bool EqualsTo(IPath path)
        {
            return _path.Equals(path);
        }

        public IPartsSequence Parts => _path.Parts;
    }
}