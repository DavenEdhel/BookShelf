using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BookWiki.Core.Search
{
    public class NamesByGroups : IEnumerable<GroupedNames>
    {
        private readonly INovel _novel;

        public NamesByGroups(INovel novel)
        {
            _novel = novel;
        }

        public IEnumerator<GroupedNames> GetEnumerator()
        {
            var document = new DocumentFlowContentFromTextAndFormat(_novel.Content, _novel.Format);

            var result = new List<List<IParagraph>>();

            var section = new List<IParagraph>();

            foreach (var p in document.Paragraphs)
            {
                if (p.FormattingStyle == TextStyle.Right)
                {
                    result.Add(section);
                    section = new List<IParagraph>();
                    section.Add(p);
                }
                else
                {
                    section.Add(p);
                }
            }

            result.Add(section);

            foreach (var p in result.Where(x => x.Count > 1 && x.Any(y => y.FormattingStyle == TextStyle.Right)))
            {
                yield return new GroupedNames(p.First(x => x.FormattingStyle == TextStyle.Right).GetAllText(), p.Skip(1).Select(x => x.GetAllText()).ToArray());
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}