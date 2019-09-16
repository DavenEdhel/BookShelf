using BookWiki.Core.Files.PathModels;

namespace BookWiki.Core
{
    public class SearchableContent : ISearchableContent
    {
        private readonly IContent _content;

        private ISequence<int> _sentenceStartIndexes;
        private ISequence<int> _paragraphStartIndexes;

        public SearchableContent(IContent content)
        {
            _content = content;
        }

        public IText Content => _content.Content;

        public string Title => _content.Title;

        public IRelativePath Source => _content.Source;

        public ISequence<int> SentenceStartIndexes => _sentenceStartIndexes ?? (_sentenceStartIndexes = new RunOnceSequence<int>(new IndexSequence(Content.PlainText, '.', ',', '!')));

        public ISequence<int> ParagraphsStartIndexes => _paragraphStartIndexes ?? (_paragraphStartIndexes = new RunOnceSequence<int>(new IndexSequence(Content.PlainText, '\n')));
    }
}