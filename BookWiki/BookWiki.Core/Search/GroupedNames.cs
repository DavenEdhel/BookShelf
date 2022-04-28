using System.Collections;
using System.Collections.Generic;

namespace BookWiki.Core.Search
{
    public class GroupedNames : IEnumerable<NamesWithFullContent>
    {
        private readonly IEnumerable<string> _ps;

        public GroupedNames(string group, IEnumerable<string> ps)
        {
            Group = @group;
            _ps = ps;
        }

        public string Group { get; }

        public bool IsValid { get; set; } = true;

        public IEnumerator<NamesWithFullContent> GetEnumerator()
        {
            foreach (var p in _ps)
            {
                yield return new NamesWithFullContent(p);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}