using BookWiki.Core.Files.FileSystemModels;
using BookWiki.Core.Files.PathModels;

namespace BookWiki.Core.Files.FileModels
{
    public class NovelFolder
    {
        private ContentFolder _contentFolder;

        public NovelFolder(IPath root, string name)
        {
            var path = new FolderPath(root, new FileNameWithoutExtension(name), new Extension(NodeType.Novel));

            _contentFolder = new ContentFolder(path);
        }

        public void Save(INovel novel)
        {
            _contentFolder.Save(novel.Format);
            _contentFolder.Save(novel.Content);
        }
    }
}