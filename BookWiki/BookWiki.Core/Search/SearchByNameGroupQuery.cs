using System.Collections.Generic;
using System.Linq;

namespace BookWiki.Core.Search
{
    public class SearchByNameGroupQuery
    {
        private readonly INovel _novel;
        private readonly string _query;

        public SearchByNameGroupQuery(INovel novel, string query)
        {
            _novel = novel;
            _query = query;
        }

        public IEnumerable<string> Execute()
        {
            var allNames = new NamesByGroups(_novel).ToArray();

            foreach (var nameWithContent in allNames.Where(x => x.IsValid))
            {
                if (nameWithContent.Group.ToLower().Contains(_query.ToLower()))
                {
                    foreach (var groupedName in nameWithContent)
                    {
                        yield return groupedName.FullContent;
                    }
                }
            }
        }
    }
}