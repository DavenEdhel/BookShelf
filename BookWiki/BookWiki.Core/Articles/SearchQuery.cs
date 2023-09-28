using System;
using System.Collections.Generic;
using System.Linq;

namespace BookWiki.Core.Articles
{
    public class SearchQuery
    {
        private readonly string _query;

        public SearchQuery(string query)
        {
            _query = query;

            if (string.IsNullOrWhiteSpace(_query))
            {
                NamePart = "*";
                Tags = Array.Empty<string>();
            }
            else
            {
                var parts = _query.Split(' ');
                NamePart = parts.First();
                Tags = parts.Skip(1).ToArray();
            }
        }

        public string NamePart { get; }

        public string[] Tags { get; }
    }

    public interface INameSearchQuery
    {
        bool IsWildcard { get; }

        bool IsExact { get; }

        string Query { get; }
    }

    public class Wildcard : INameSearchQuery
    {
        public bool IsWildcard { get; } = true;
        public bool IsExact { get; } = false;
        public string Query { get; } = string.Empty;
    }

    public class NameSearchQuery : INameSearchQuery
    {
        private readonly string _name;

        public NameSearchQuery(string name)
        {
            _name = name;

            bool isAllOrName = false;
            bool isExact = false;

            var remains = _name.ToLowerInvariant();
            if (_name[0] == '*')
            {
                remains = remains.Replace("*", "");
                if (string.IsNullOrWhiteSpace(remains))
                {
                    isAllOrName = true;
                }
            }

            if (string.IsNullOrWhiteSpace(remains) == false)
            {
                if (remains[0] == '!')
                {
                    isExact = true;
                    remains = remains.Replace("!", "");
                }
            }

            if (string.IsNullOrWhiteSpace(remains) == false)
            {
                if (isExact)
                {
                    IsWildcard = false;
                    IsExact = true;
                    Query = remains;
                }
                else
                {
                    IsWildcard = false;
                    IsExact = false;
                    Query = remains;
                }
            }
            else if (isExact)
            {
                IsExact = true;
                IsWildcard = false;
                Query = string.Empty;
            }
            else if (isAllOrName)
            {
                IsExact = false;
                IsWildcard = true;
                Query = string.Empty;
            }
        }

        public bool IsWildcard { get; }

        public bool IsExact { get; }

        public string Query { get; }
    }

    public class SearchQueryV2
    {
        public SearchQueryV2(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                Name = new Wildcard();
                Tags = Array.Empty<string>();
            }
            else
            {
                var items = query.Split(' ').Select(x => x.ToLowerInvariant()).ToArray();

                if (string.IsNullOrWhiteSpace(items[0]) == false)
                {
                    Name = new NameSearchQuery(items[0]);
                    Tags = items.Where(x => string.IsNullOrWhiteSpace(x) == false).Skip(1).ToArray();
                }
                else
                {
                    Name = new Wildcard();
                    Tags = Array.Empty<string>();

                    var result = new List<string>();
                    foreach (var tagOrName in items.Skip(1))
                    {
                        if (string.IsNullOrWhiteSpace(tagOrName))
                        {
                            continue;
                        }

                        if (tagOrName[0] == '*')
                        {
                            Name = new NameSearchQuery(tagOrName);
                        }
                        else
                        {
                            result.Add(tagOrName);
                        }
                    }

                    Tags = result.ToArray();
                }
            }
        }

        public INameSearchQuery Name { get; }

        public string[] Tags { get; }
    }
}