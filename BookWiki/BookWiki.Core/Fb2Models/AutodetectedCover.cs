using System;
using System.IO;
using BookWiki.Core.Files.PathModels;

namespace BookWiki.Core.Fb2Models
{
    public class AutodetectedCover : IBase64
    {
        private readonly IAbsolutePath _pathToFolderToSearch;

        public AutodetectedCover(IAbsolutePath pathToFolderToSearch)
        {
            _pathToFolderToSearch = pathToFolderToSearch;

            var coverPath = new AutodetectCoverFilePath(pathToFolderToSearch);

            if (coverPath.Value != null)
            {
                var bytes = File.ReadAllBytes(coverPath.Value.FullPath);

                var base64 = Convert.ToBase64String(bytes);

                Value = base64;
            }
        }

        public string Value { get; } = string.Empty;
    }
}