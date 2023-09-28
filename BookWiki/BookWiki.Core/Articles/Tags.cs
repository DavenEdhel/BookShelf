using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;

namespace BookWiki.Core.Articles
{
    public class Tags : IObservable<Tags>
    {
        private readonly ArticlesLibrary _library;
        private readonly Subject<Tags> _changed = new Subject<Tags>();

        public Tags(ArticlesLibrary library)
        {
            _library = library;
        }

        public IEnumerable<TagSearchResult> Search(string query, string[] clarifications)
        {
            EnsureTagsLoaded();

            clarifications = clarifications.Select(x => x.ToLowerInvariant()).ToArray();

            var updatedExceptions = clarifications.Where(x => x != query).ToArray();

            var q = new SearchQueryV2(query);

            var filteredArticles = FilterByName(q.Name);

            filteredArticles = filteredArticles.Where(x => updatedExceptions.Contains(x.Tag.Name.ToLowerInvariant()) == false).ToArray();

            ApplyClarificationScore(filteredArticles);

            //ApplyClarifications(filteredArticles);

            //var filteredArticlesWithClarifications = FilterOutByClarification(filteredArticles).Distinct().ToArray();

            //if (filteredArticlesWithClarifications.Any())
            //{
            //    filteredArticles = filteredArticlesWithClarifications;
            //}

            var result = filteredArticles.OrderByDescending(x => x.ClarificationScore).ThenByDescending(x => x.Score).ThenByDescending(x => x.Tag.Usage).ToArray();

            var i = 1;

            foreach (var tagSearchResult in result)
            {
                tagSearchResult.Index = i;
                i++;
            }

            return result;

            TagSearchResult WithDefaultScore(Tag tag)
            {
                return new TagSearchResult()
                {
                    Tag = tag,
                    Score = 0
                };
            }

            TagSearchResult[] FilterByName(INameSearchQuery namePart)
            {
                if (namePart.IsWildcard)
                {
                    return GetAllByName(x => true).Select(WithDefaultScore).ToArray();
                }

                if (namePart.IsExact)
                {
                    return GetAllByName(x =>
                        {
                            if (x.ToLowerInvariant().Equals(namePart.Query))
                            {
                                return true;
                            }

                            return false;
                        }
                    ).Select(WithDefaultScore).ToArray();
                }

                var result = new List<TagSearchResult>();

                foreach (var article in GetAllByName(x => true))
                {
                    AddToResult(article);
                }

                void AddToResult(Tag tag)
                {
                    if (tag.Name.ToLowerInvariant().Equals(namePart.Query))
                    {
                        result.Add(new TagSearchResult()
                        {
                            Tag = tag,
                            Score = 10
                        });
                        return;
                    }

                    if (tag.Name.ToLowerInvariant().Contains(namePart.Query))
                    {
                        result.Add(new TagSearchResult()
                        {
                            Tag = tag,
                            Score = 1
                        });

                        return;
                    }
                }

                return result.ToArray();
            }

            void ApplyClarifications(TagSearchResult[] result)
            {
                foreach (var tagSearchResult in result)
                {
                    if (TagAppliedToAllClarifications(tagSearchResult.Tag))
                    {
                        tagSearchResult.Score += 100;
                    }
                }
            }

            void ApplyClarificationScore(TagSearchResult[] result)
            {
                foreach (var tagSearchResult in result)
                {
                    var weight = 1;
                    foreach (var clarification in clarifications)
                    {
                        if (tagSearchResult.Tag.Relatives.Contains(clarification))
                        {
                            tagSearchResult.ClarificationScore += weight;
                        }

                        //weight++;
                    }
                }
            }

            IEnumerable<TagSearchResult> FilterOutByClarification(TagSearchResult[] result)
            {
                foreach (var tagSearchResult in result)
                {
                    if (TagAppliedToAllClarifications(tagSearchResult.Tag))
                    {
                        yield return tagSearchResult;
                    }
                }
            }

            bool TagAppliedToAllClarifications(Tag tag)
            {
                foreach (var clarification in clarifications)
                {
                    if (tag.Relatives.Contains(clarification) == false)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        private void EnsureTagsLoaded()
        {
            if (_table.Any())
            {
                return;
            }

            var allArticles = _library.Search("*").ToArray();

            foreach (var articleSearchResult in allArticles)
            {
                foreach (var articleTag in articleSearchResult.Article.Tags)
                {
                    _table.Add(new ArticleAndTag()
                    {
                        ArticleId = articleSearchResult.Article.Source.FullPath,
                        Tag = articleTag,
                        Related = articleSearchResult.Article.Tags
                    });
                }
            }
        }

        private readonly List<ArticleAndTag> _table = new List<ArticleAndTag>();

        private IEnumerable<Tag> GetAllByName(Func<string, bool> nameFilter)
        {
            return _table.GroupBy(x => x.Tag)
                .Where(x => string.IsNullOrWhiteSpace(x.Key) == false)
                .Where(x => nameFilter(x.Key))
                .Select(
                    x => new Tag()
                    {
                        Name = x.Key,
                        Usage = x.Count(),
                        Relatives = x.SelectMany(e => e.Related).Select(x => x.ToLowerInvariant()).Distinct().ToArray()
                    }
                ).ToArray();
        }

        public void ArticleSaved(Article article)
        {
            _table.RemoveAll(x => x.ArticleId == article.Source.FullPath);

            foreach (var articleTag in article.Tags)
            {
                _table.Add(new ArticleAndTag()
                {
                    ArticleId = article.Source.FullPath,
                    Tag = articleTag,
                    Related = article.Tags
                });
            }

            _changed.OnNext(this);
        }

        public IDisposable Subscribe(IObserver<Tags> observer)
        {
            return _changed.Subscribe(observer);
        }

        private class ArticleAndTag
        {
            public string ArticleId { get; set; }

            public string Tag { get; set; }

            public string[] Related { get; set; }
        }
    }
}