using System;
using BookWiki.Core.Files.PathModels;

namespace BookWiki.Presentation.Apple.Models
{
    public class UserFolderPath : IRootPath
    {
        private readonly IPath _path;

        public UserFolderPath()
        {
            _path = new FolderPath(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
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