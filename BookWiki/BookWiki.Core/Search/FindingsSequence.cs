using System;
using System.Collections;
using System.Collections.Generic;
using BookWiki.Core.Findings;

namespace BookWiki.Core
{
    public class FindingsSequence : ISequence<IFinding>
    {
        private readonly ISearchableContent _article;

        private readonly IQuery _query;

        private readonly string _content;

        public Progress Completion { get; }

        public FindingsSequence(IContent article, IQuery query)
        {
            _article = new SearchableContent(article);
            _query = query;
            _content = _article.Content.PlainText;

            Completion = new Progress(_content.Length);
        }

        public IEnumerator<IFinding> GetEnumerator()
        {
            var lastIndex = 0;
            int findingIndex;

            do
            {
                findingIndex = _content.IndexOf(_query.PlainText, lastIndex, StringComparison.Ordinal);

                if (findingIndex != -1)
                {
                    lastIndex = findingIndex + _query.PlainText.Length;

                    Completion.Change(lastIndex);

                    yield return new SmartContextFinding(_article, findingIndex, _query.PlainText.Length);
                }

            } while (findingIndex != -1);

            Completion.MarkCompleted();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}