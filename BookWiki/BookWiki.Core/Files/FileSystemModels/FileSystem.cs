using System.Collections.Generic;
using System.IO;
using System.Linq;
using BookWiki.Core.Files.PathModels;
using BookWiki.Core.FileSystem.PathModels;

namespace BookWiki.Core.Files.FileSystemModels
{
    public class FileSystem : IFileSystem
    {
        private readonly string _root;

        public ISequence<IPath> Contents => new ArraySequence<IPath>(GetContents().ToArray());

        public FileSystem(string root)
        {
            _root = root;

        }

        private IEnumerable<IPath> GetContents()
        {
            var files = Directory.GetFiles(_root, "*", SearchOption.AllDirectories);

            var dirs = files
                .Select(file =>
                {
                    var parts = file.Split(Path.DirectorySeparatorChar).ToArray();

                    var dir = string.Join("" + Path.DirectorySeparatorChar, parts.Take(parts.Length - 1).ToArray());

                    return dir;
                })
                .Distinct()
                .ToArray();

            return dirs.Select(x => new FolderPath(x));
        }
    }
}