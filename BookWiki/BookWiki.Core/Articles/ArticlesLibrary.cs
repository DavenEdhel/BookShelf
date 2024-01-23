using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Subjects;
using BookWiki.Core.Files.FileModels;
using BookWiki.Core.Files.FileSystemModels;
using BookWiki.Core.Files.PathModels;
using BookWiki.Core.Utils.PropertyModels;

namespace BookWiki.Core.Articles
{
    public class ArticlesLibrary : ILex
    {
        private readonly IRootPath _rootPath;
        private List<Article> _allArticles;
        private readonly Subject<Unit> _changed = new Subject<Unit>();
        private readonly CachedValue<IEnumerable<string>> _words;

        public ArticlesLibrary(IRootPath rootPath)
        {
            _rootPath = rootPath;
            _words = new CachedValue<IEnumerable<string>>(
                () => Search("*").SelectMany(x => x.Article.NameVariations).Select(x => x.ToLowerInvariant()).ToArray()
            );

            ArticlesRoot.EnsureCreated();

            Tags = new Tags(this);
        }

        public IObservable<Unit> Changed => _changed;

        public IEnumerable<ArticleSearchResult> Search(string query, string[] scope = null)
        {
            scope ??= Array.Empty<string>();

            EnsureArticlesLoaded();

            var q = new SearchQueryV2(query);

            var filteredArticles = FilterByName(q.Name);

            filteredArticles = FilterByScope(filteredArticles);

            if (q.Tags.Length > 0)
            {
                AdjustByTags(filteredArticles);
            }

            return filteredArticles.OrderByDescending(x => x.Score).ThenBy(x => x.Article.Name).ToArray();

            ArticleSearchResult WithDefaultScore(Article article)
            {
                return new ArticleSearchResult()
                {
                    Article = article,
                    Score = 0
                };
            }

            ArticleSearchResult[] FilterByName(INameSearchQuery namePart)
            {
                if (namePart.IsWildcard)
                {
                    return _allArticles.Select(WithDefaultScore).ToArray();
                }

                if (namePart.IsExact)
                {
                    return _allArticles.Where(x =>
                        {
                            if (x.Name.ToLowerInvariant().Equals(namePart.Query))
                            {
                                return true;
                            }

                            foreach (var argNameVariation in x.NameVariations)
                            {
                                if (argNameVariation.ToLowerInvariant().Equals(namePart.Query))
                                {
                                    return true;
                                }
                            }

                            return false;
                        }
                    ).Select(WithDefaultScore).ToArray();
                }

                var result = new List<ArticleSearchResult>();

                foreach (var article in _allArticles)
                {
                    AddToResult(article);
                }

                void AddToResult(Article article)
                {
                    if (article.Name.ToLowerInvariant().Equals(namePart.Query))
                    {
                        result.Add(new ArticleSearchResult()
                        {
                            Article = article,
                            Score = 10
                        });
                        return;
                    }

                    foreach (var argNameVariation in article.NameVariations)
                    {
                        if (argNameVariation.ToLowerInvariant().Equals(namePart.Query))
                        {
                            result.Add(new ArticleSearchResult()
                            {
                                Article = article,
                                Score = 10
                            });

                            return;
                        }
                    }

                    if (article.Name.ToLowerInvariant().Contains(namePart.Query))
                    {
                        result.Add(new ArticleSearchResult()
                        {
                            Article = article,
                            Score = 1
                        });
                        return;
                    }

                    foreach (var argNameVariation in article.NameVariations)
                    {
                        if (argNameVariation.ToLowerInvariant().Contains(namePart.Query))
                        {
                            result.Add(new ArticleSearchResult()
                            {
                                Article = article,
                                Score = 1
                            });

                            return;
                        }
                    }
                }

                return result.ToArray();
            }

            ArticleSearchResult[] FilterByScope(ArticleSearchResult[] results)
            {
                if (scope == null || scope.Any() == false)
                {
                    return results;
                }

                return results.Where(
                    x =>
                    {
                        return scope.All(t => x.Article.Tags.Any(at => t.Equals(at)));

                    }).ToArray();
            }

            void AdjustByTags(ArticleSearchResult[] results)
            {
                foreach (var articleSearchResult in results)
                {
                    foreach (var tag in q.Tags)
                    {
                        if (articleSearchResult.Article.Tags.Any(artTag => artTag.Equals(tag)))
                        {
                            articleSearchResult.Score += 10;
                            articleSearchResult.MatchedTags.Add(tag);
                        }
                        else if (articleSearchResult.Article.Tags.Any(artTag => artTag.Contains(tag)))
                        {
                            foreach (var sourceTag in articleSearchResult.Article.Tags.Where(artTag => artTag.Contains(tag)))
                            {
                                articleSearchResult.Score += 1;
                                articleSearchResult.PartiallyMatchedTags.Add(sourceTag);
                            }
                        }
                    }
                }
            }
        }

        public Article Get(IRelativePath path)
        {
            EnsureArticlesLoaded();

            return _allArticles.First(x => x.Source.EqualsTo(path));
        }

        public Article TryGet(string id)
        {
            EnsureArticlesLoaded();

            return _allArticles.FirstOrDefault(x => x.Id == id);
        }

        public Article New()
        {
            var articleAbsolutePath = new FolderPath(
                ArticlesRoot,
                new FileName(Guid.NewGuid().ToString().Replace("-", "")),
                new Extension(NodeType.Article)
            );

            var article = new Article(
                
                articleAbsolutePath.RelativePath(_rootPath),

                _rootPath
            );

            new ContentFolder(articleAbsolutePath).Save(new EmptySubstring());

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

        public IEnumerable<string> Words => _words.Value;

        public Tags Tags { get; }

        public void Save(Article article)
        {
            _words.Invalidate();
            Tags.ArticleSaved(article);
        }
    }
}