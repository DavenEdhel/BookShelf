using System.IO;
using BookWiki.Core.Files.PathModels;

namespace BookWiki.Core.Fb2Models
{
    public class Fb2File
    {
        private readonly string _content;
        private IAbsolutePath _pathToSave;

        public Fb2File(IAbsolutePath path, string fileName, string content)
        {
            _content = content;
            _pathToSave = new FilePath(path, new UniqueName(path, fileName, "fb2").Value);
        }

        public void Save()
        {
            File.WriteAllText(_pathToSave.FullPath, _content);
        }
    }
}