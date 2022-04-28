using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BookWiki.Core.Search
{
    public class AllNames : IEnumerable<NamesWithFullContent>
    {
        private readonly INovel _novel;

        public AllNames(INovel novel)
        {
            _novel = novel;
        }

        public IEnumerator<NamesWithFullContent> GetEnumerator()
        {
            var document = new DocumentFlowContentFromTextAndFormat(_novel.Content, _novel.Format);

            var content = document.Paragraphs.Where(x => x.FormattingStyle == TextStyle.None);

            foreach (var p in content)
            {
                yield return new NamesWithFullContent(p.GetAllText());
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}