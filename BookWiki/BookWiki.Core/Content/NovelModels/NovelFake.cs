using BookWiki.Core.Files.PathModels;

namespace BookWiki.Core
{
    public class NovelFake : INovel
    {
        public IText Content { get; set; }
        public string Title { get; set; }
        public IRelativePath Source { get; set; }
        public ISequence<ITextInfo> Format { get; set; }
        public IText Comments { get; set; }
    }
}