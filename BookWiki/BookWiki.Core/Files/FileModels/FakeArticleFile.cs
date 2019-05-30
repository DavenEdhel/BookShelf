using BookWiki.Core.Files.PathModels;
using BookWiki.Core.FileSystem.PathModels;

namespace BookWiki.Core
{
    public class FakeArticleFile
    {
        private readonly IPath _path;

        public FakeArticleFile(IPath path)
        {
            _path = path;
        }

        public void SaveContent(IText articleContent)
        {
            
        }
    }
}