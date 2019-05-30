using BookWiki.Core.FileSystem.FileModels;
using BookWiki.Core.Utils.PropertyModels;

namespace BookWiki.Core.Files.FileModels
{
    public class CachedTextFile : IFile
    {
        private readonly IFile _file;

        private CachedValue<string> _content;

        public CachedTextFile(IFile file)
        {
            _file = file;
            _content = new CachedValue<string>(() => _file.Content);
        }

        public void Save(string content)
        {
            _file.Save(content);
            _content.Invalidate();
        }

        public string Content => _content.Value;
    }
}