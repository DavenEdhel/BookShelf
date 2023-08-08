using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Subjects;
using BookWiki.Core.Files.FileSystemModels;
using BookWiki.Core.Files.PathModels;

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

    public interface ILex
    {
        IEnumerable<string> Words { get; }
    }

    public class EmptyLex : ILex
    {
        public IEnumerable<string> Words => Array.Empty<string>();
    }

    public class ArticlesLibrary : ILex
    {
        private readonly IRootPath _rootPath;
        private List<Article> _allArticles;
        private readonly Subject<Unit> _changed = new Subject<Unit>();

        public ArticlesLibrary(IRootPath rootPath)
        {
            _rootPath = rootPath;
        }

        public IObservable<Unit> Changed => _changed;

        public IEnumerable<Article> Search(string query)
        {
            EnsureArticlesLoaded();

            var q = new SearchQuery(query);

            var filteredArticles = FilterByName(q.NamePart);


            if (q.NamePart != "*")
            {
                filteredArticles = filteredArticles.OrderBy(x => x.Name.Length).ToArray();
            }

            if (q.Tags.Length > 0)
            {
                filteredArticles = filteredArticles.OrderByDescending(x => q.Tags.Count(tag => x.Tags.Contains(tag))).ToArray();
            }

            return filteredArticles;

            Article[] FilterByName(string namePart)
            {
                if (namePart == "*")
                {
                    return _allArticles.ToArray();
                }

                if (namePart[0] == '!')
                {
                    var n = namePart.Replace("!", string.Empty);

                    return _allArticles.Where(x =>
                    {
                        if (x.Name.ToLowerInvariant().Equals(n.ToLowerInvariant()))
                        {
                            return true;
                        }

                        foreach (var argNameVariation in x.NameVariations)
                        {
                            if (argNameVariation.ToLowerInvariant().Equals(n.ToLowerInvariant()))
                            {
                                return true;
                            }
                        }

                        return false;
                    }
                    ).ToArray();
                }
                else
                {
                    return _allArticles.Where(x =>
                    {
                        if (x.Name.ToLowerInvariant().Contains(namePart.ToLowerInvariant()))
                        {
                            return true;
                        }

                        foreach (var argNameVariation in x.NameVariations)
                        {
                            if (argNameVariation.ToLowerInvariant().Contains(namePart.ToLowerInvariant()))
                            {
                                return true;
                            }
                        }

                        return false;
                    }
                    ).ToArray();
                }
            }
        }

        public Article Get(IRelativePath path)
        {
            EnsureArticlesLoaded();

            return _allArticles.First(x => x.Source.EqualsTo(path));
        }

        public Article New()
        {
            var article = new Article(
                new FolderPath(
                        ArticlesRoot,
                        new FileName(Guid.NewGuid().ToString().Replace("-", "")),
                        new Extension(NodeType.Article)
                    )
                    .RelativePath(_rootPath),

                _rootPath
            );

            _allArticles.Add(article);

            _changed.OnNext(Unit.Default);

            return article;
        }

        private void InvalidateCache()
        {
            _allArticles = null;
        }

        private void EnsureArticlesLoaded()
        {
            if (_allArticles == null)
            {
                _allArticles = new FileSystemNode(
                    ArticlesRoot.FullPath
                ).InnerNodes.Select(x => new Article(x.Path.RelativePath(_rootPath), _rootPath)).ToList();
            }
        }

        private FolderPath ArticlesRoot => new FolderPath(
            _rootPath,
            new FileName("Статьи"),
            new Extension(NodeType.Directory)
        );

        public IEnumerable<string> Words => Search("*").SelectMany(x => x.NameVariations).Select(x => x.ToLowerInvariant()).ToArray();
    }
}