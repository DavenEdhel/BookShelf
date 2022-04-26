using System.IO;
using System.Linq;
using BookWiki.Core.Files.PathModels;

namespace BookWiki.Core.Fb2Models
{
    public class AutodetectCoverFilePath
    {
        public AutodetectCoverFilePath(IAbsolutePath pathToFolderToSearch)
        {
            var covers = Directory.GetFiles(pathToFolderToSearch.FullPath)
                .Where(x => Path.GetFileNameWithoutExtension(x).Contains("cover")).ToArray();

            if (covers.Any())
            {
                var cover = covers.Select(x => Path.GetFileNameWithoutExtension(x).Replace("cover", ""))
                    .Where(x => int.TryParse(x, out _)).Select(x => int.Parse(x)).Max();

                var filePath = new FilePath(pathToFolderToSearch, $"cover{cover}.jpg");

                if (File.Exists(filePath.FullPath))
                {
                    Value = filePath;
                }
            }
        }

        public IAbsolutePath Value { get; }
    }
}