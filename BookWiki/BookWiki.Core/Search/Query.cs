using System.Linq;

namespace BookWiki.Core.Search
{
    public class Query
    {
        public Query(string query)
        {
            var parts = query.Split(' ');

            Command = parts[0];

            if (parts.Length > 1)
            {
                Arguments = string.Join(" ", parts.Skip(1));
            }
        }

        public string Command { get; }

        public string Arguments { get; }
    }
}