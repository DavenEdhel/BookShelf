using System.Collections.Generic;
using System.Linq;

namespace BookWiki.Core.Search
{
    public class SearchByNamePartQuery
    {
        private readonly INovel _novel;
        private readonly string _query;

        public SearchByNamePartQuery(INovel novel, string query)
        {
            _novel = novel;
            _query = query;
        }

        public IEnumerable<string> Execute()
        {
            var allNames = new AllNames(_novel).ToArray();

            foreach (var nameWithContent in allNames.Where(x => x.IsValid))
            {
                if (nameWithContent.Names.Any(x => x.ToLower().Contains(_query.ToLower())))
                {
                    yield return nameWithContent.FullContent;
                }
            }
        }
    }
}