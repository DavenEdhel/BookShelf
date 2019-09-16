using System.Collections.Generic;
using System.Linq;
using BookWiki.Core.Files.PathModels;
using Newtonsoft.Json;

namespace BookWiki.Core.Content
{
    public class FakeArticle : IArticle
    {
        protected readonly List<ArticlePart> Parts;

        private bool _fetched = false;

        private IView _view;
        private IText _content;
        private IRelativePath _source;

        public string Title => (Parts.FirstOrDefault()?.Text ?? string.Empty).Replace("\n", "");

        public IRelativePath Source => _source;

        public IText Content => _content ?? (_content = new StringText(ToSingleString()));

        public FakeArticle(ArticleTitle title)
        {
            _source = new PathFake(title.PlainText);

            Parts = new List<ArticlePart>()
            {
                ArticlePart.CreateHeader(title.TextWithLineBreak)
            };
        }

        public FakeArticle(List<ArticlePart> parts)
        {
            _fetched = true;

            Parts = parts;
        }

        public FakeArticle(string json)
        {
            _fetched = true;

            Parts = JsonConvert.DeserializeObject<List<ArticlePart>>(json);
        }

        public void SetViewport(IView view)
        {
            _view = view;
        }

        public void RemovePart(int offset, int lengthToRemove)
        {
            var range = GetRange(offset, lengthToRemove);
            var processed = 0;

            for (int i = 0; i < range.Parts.Count; i++)
            {
                var partToDelete = range.Parts[i];

                if (i == 0)
                {
                    if (range.StartingOffset + lengthToRemove > partToDelete.Text.Length)
                    {
                        processed = partToDelete.Text.Length - range.StartingOffset;

                        partToDelete.Text = partToDelete.Text.Substring(0, range.StartingOffset);
                    }
                    else
                    {
                        var firstPart = partToDelete.Text.Substring(0, range.StartingOffset);
                        var secondPart = partToDelete.Text.Substring(range.StartingOffset + lengthToRemove, partToDelete.Text.Length - range.StartingOffset - lengthToRemove);

                        partToDelete.Text = firstPart + secondPart;

                        return;
                    }
                }
                else if (i == range.Parts.Count - 1)
                {
                    var toRemove = lengthToRemove - processed;

                    partToDelete.Text = partToDelete.Text.Substring(toRemove, partToDelete.Text.Length - toRemove);

                    return;
                }
                else
                {
                    processed += partToDelete.Text.Length;

                    Parts.Remove(partToDelete);
                }
            }
        }

        public void InsertPart(int offset, string textToInsert)
        {
            var partToInsert = GetRange(offset, 0);

            if (partToInsert.Parts.Any())
            {
                var part = partToInsert.Parts.First();

                if (part.MarkerStyle == MarkerStyle.Header && textToInsert.Contains('\n'))
                {
                    var newText = part.Text.Insert(partToInsert.StartingOffset, textToInsert);

                    var parts = newText.Split('\n');

                    if (parts.Length == 1)
                    {
                        part.Text = parts[0] + '\n';
                    }
                    else
                    {
                        part.Text = parts[0] + '\n';

                        Parts.Insert(1, ArticlePart.CreateText(parts[1] + '\n'));

                        _view?.Render();
                    }
                }
                else
                {
                    part.Text = part.Text.Insert(partToInsert.StartingOffset, textToInsert);
                }
            }
            else if (Parts.Any())
            {
                Parts.Last().Text += textToInsert;
            }
            else
            {
                Parts.Add(ArticlePart.CreateHeader(textToInsert));
            }
        }

        private ArticleRange GetRange(int offset, int length)
        {
            var start = 0;

            var result = new ArticleRange();

            foreach (var articlePart in Parts)
            {
                var end = start + articlePart.Text.Length;

                var a = offset;
                var b = offset + length;

                if (a >= start && a < end ||
                    b >= start && b < end)
                {
                    if (result.Parts.Any() == false)
                    {
                        result.StartingOffset = offset - start;
                    }

                    result.Parts.Add(articlePart);

                }

                start = end;
            }

            return result;
        }

        public string ToSingleString()
        {
            Fetch();

            return string.Join("", Parts.Select(x => x.Text));
        }

        public IEnumerable<ArticlePart> ToArticleParts()
        {
            Fetch();

            return Parts.ToArray();
        }

        private void Fetch()
        {
            if (_fetched == false)
            {
                _fetched = true;

                for (int i = 0; i < 25; i++)
                {
                    Parts.Add(ArticlePart.CreateText(ContentHelper.GenerateParagraph()));
                }
            }
        }

        class ArticleRange
        {
            public List<ArticlePart> Parts { get; set; } = new List<ArticlePart>();

            public int StartingOffset { get; set; }
        }
    }
}