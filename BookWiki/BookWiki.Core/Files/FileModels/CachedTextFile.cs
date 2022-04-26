using BookWiki.Core.FileSystem.FileModels;
using BookWiki.Core.Utils.PropertyModels;

namespace BookWiki.Core.Files.FileModels
{
    public class CachedTextFile : IFile
    {
        private readonly IFile _file;

        private CachedValue<string> _content;

        private CachedValue<string[]> _lines;

        public CachedTextFile(IFile file)
        {
            _file = file;
            _content = new CachedValue<string>(() => _file.Content);
            _lines = new CachedValue<string[]>(() => _file.Lines);
        }

        public void Save(string content)
        {
            _file.Save(content);
            _content.Invalidate();
        }

        public string Content => _content.Value;

        public string[] Lines => _lines.Value;
    }
}