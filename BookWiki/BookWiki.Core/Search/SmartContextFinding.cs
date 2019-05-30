using System;
using System.Linq;
using BookWiki.Core.Search;

namespace BookWiki.Core.Findings
{
    public class SmartContextFinding : Finding
    {
        private readonly ISearchableContent _article;
        private readonly int _findingIndex;
        private readonly int _findingLength;

        public SmartContextFinding(ISearchableContent article, int findingIndex, int findingLength) : base(findingIndex, findingLength)
        {
            _article = article;
            _findingIndex = findingIndex;
            _findingLength = findingLength;
        }

        protected override ITextRange CalculateResult()
        {
            return _article.Content.Substring(_findingIndex, _findingLength);
        }

        protected override ITextRange CalculateContext()
        {
            if (_article.Content.Length < 300)
            {
                return _article.Content.Substring(0, _article.Content.Length);
            }

            var start = FindStartParagraph();
            var end = FindEndParagraph();
            var initialParagraphLength = end - start;

            if (initialParagraphLength < 300 && initialParagraphLength > 150)
            {
                return _article.Content.Substring(start, initialParagraphLength);
            }

            start = NormalizeStart(start);
            end = NormalizeEnd(end);

            return _article.Content.Substring(start, end - start);
        }

        private int FindStartParagraph()
        {
            var lastParagraphIndex = 0;

            foreach (var paragraph in _article.ParagraphsStartIndexes)
            {
                if (paragraph >= _findingIndex)
                {
                    return lastParagraphIndex;
                }

                lastParagraphIndex = paragraph;
            }

            return lastParagraphIndex;
        }

        private int FindEndParagraph()
        {
            if (_article.ParagraphsStartIndexes.Any(paragraph => paragraph >= _findingIndex))
            {
                return _article.ParagraphsStartIndexes.FirstOrDefault(paragraph => paragraph >= _findingIndex);
            }

            return _article.Content.Length;
        }

        private int NormalizeStart(int start)
        {
            var indexes = _article.SentenceStartIndexes.Where(x => x < _findingIndex).Reverse().ToArray();

            if (indexes.Any(x => _findingIndex - x > 100 && _findingIndex - x < 150))
            {
                return indexes.First(x => _findingIndex - x > 100 && _findingIndex - x < 150);
            }

            return start;
        }

        private int NormalizeEnd(int end)
        {
            var indexes = _article.SentenceStartIndexes.Where(x => x > _findingIndex).ToArray();

            if (indexes.Any(x => x - _findingIndex > 100 && x - _findingIndex < 150))
            {
                return indexes.First(x => x - _findingIndex > 100 && x - _findingIndex < 150);
            }

            return end;
        }
    }
}