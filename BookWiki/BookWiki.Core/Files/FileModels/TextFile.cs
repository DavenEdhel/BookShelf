using System.IO;
using BookWiki.Core.Files.PathModels;
using BookWiki.Core.FileSystem.FileModels;

namespace BookWiki.Core.Files.FileModels
{
    public class TextFile : IFile
    {
        private readonly IPath _pathToFile;

        public TextFile(IPath pathToFile)
        {
            _pathToFile = pathToFile;
        }

        public void Save(string content)
        {
            File.WriteAllText(_pathToFile.FullPath, content);
        }

        public string Content
        {
            get
            {
                if (File.Exists(_pathToFile.FullPath))
                {
                    return File.ReadAllText(_pathToFile.FullPath);
                }

                return string.Empty;
            }
        } 
    }
}