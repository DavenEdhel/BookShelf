using BookWiki.Core.Files.PathModels;

namespace BookWiki.Presentation.Wpf.Models
{
    public class RootPath : IRootPath
    {
        private readonly IPath _path;

        public RootPath()
        {
            _path = new FolderPath(@"C:\Work\Projects\BookShelf\BookWiki\BookWiki.Presentation.Wpf\bin\Debug\Book Wiki 4");
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