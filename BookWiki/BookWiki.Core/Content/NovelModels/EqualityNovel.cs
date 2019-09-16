using BookWiki.Core.Files.PathModels;

namespace BookWiki.Core.Content
{
    public class EqualityNovel : INovel
    {
        private readonly INovel _content;

        public EqualityNovel(INovel content)
        {
            _content = content;
        }

        public IText Content => _content.Content;

        public string Title => _content.Title;

        public IRelativePath Source => _content.Source;

        public ISequence<ITextInfo> Format => _content.Format;

        public override int GetHashCode()
        {
            return (Source != null ? Source.GetHashCode() : 0);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj is IContent content)
            {
                return content.Source.EqualsTo(Source);
            }

            return false;
        }

        
    }
}