using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BookWiki.Core;
using BookWiki.Core.Articles;
using BookWiki.Core.Utils;
using BookWiki.Core.Utils.TextModels;
using BookWiki.Presentation.Wpf.Models;
using Keurig.IQ.Core.CrossCutting.Extensions;

namespace BookWiki.Presentation.Wpf.Views
{
    public class TagsOverviewBehavior : IDisposable
    {
        private readonly TextBlock Suggestions;
        private TagSearchResult[] _tags;
        private readonly IDisposable _tagsSubscription;

        public TagsOverviewBehavior(TextBlock suggestions)
        {
            Suggestions = suggestions;

            _tagsSubscription = BooksApplication.Instance.Articles.Tags.Subscribe(FillTags);

            FillTags(BooksApplication.Instance.Articles.Tags);
        }

        public void FillTags(Tags tags)
        {
            _tags = tags.Search(string.Empty, clarifications: Array.Empty<string>()).ToArray();

            Suggestions.Text = _tags
                .Select(x => $"{x.Tag.Name} ({x.Tag.Usage})")
                .JoinStringsWithoutSkipping(";\n");
        }

        public void Dispose()
        {
            _tagsSubscription.Dispose();
        }
    }

    public class TagsSuggestionsBehavior : IDisposable
    {
        private readonly TextBox QueryBox;
        private readonly TextBlock Suggestions;
        private TagSearchResult[] _tags;
        private readonly CompositeDisposable _subscription = new CompositeDisposable();
        private readonly IArticleScope _scope;

        public TagsSuggestionsBehavior(
            TextBox queryBox,
            TextBlock suggestions,
            IArticleScope scope
        )
        {
            QueryBox = queryBox;
            Suggestions = suggestions;

            _scope = scope;

            _scope.ScopeChanged.Subscribe(
                _ =>
                {
                    FillTags(BooksApplication.Instance.Articles.Tags);
                }
            ).InScopeOf(_subscription);

            BooksApplication.Instance.Articles.Tags.Subscribe(FillTags).InScopeOf(_subscription);

            QueryBox.TextChanged += QueryBox_OnTextChanged;
            QueryBox.SelectionChanged += QueryBoxOnSelectionChanged;
            QueryBox.PreviewKeyDown += QueryBox_OnPreviewKeyDown;
        }

        private void QueryBoxOnSelectionChanged(object sender, RoutedEventArgs e)
        {
            FillTags(BooksApplication.Instance.Articles.Tags);
        }

        private void QueryBox_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.D1)
            {
                if (_tags.Length >= 1)
                {
                    AutocompleteTag(_tags[0]);
                }

                e.Handled = true;
            }

            if (e.Key == Key.D2)
            {
                if (_tags.Length >= 2)
                {
                    AutocompleteTag(_tags[1]);
                }

                e.Handled = true;
            }

            if (e.Key == Key.D3)
            {
                if (_tags.Length >= 3)
                {
                    AutocompleteTag(_tags[2]);
                }

                e.Handled = true;
            }

            if (e.Key == Key.D4)
            {
                if (_tags.Length >= 4)
                {
                    AutocompleteTag(_tags[3]);
                }

                e.Handled = true;
            }

            if (e.Key == Key.D5)
            {
                if (_tags.Length >= 5)
                {
                    AutocompleteTag(_tags[4]);
                }

                e.Handled = true;
            }
        }

        private void AutocompleteTag(TagSearchResult tag)
        {
            if (string.IsNullOrWhiteSpace(QueryBox.Text))
            {
                QueryBox.Text = " " + tag.Tag.Name + " ";
                QueryBox.SelectionStart = QueryBox.Text.Length;

                return;
            }

            var currentWord = GetCurrentWord();

            QueryBox.Select(currentWord.Offset, currentWord.Length());
            if (QueryBox.Text.Length == currentWord.Offset + currentWord.Length())
            {
                QueryBox.SelectedText = tag.Tag.Name + " ";
            }
            else
            {
                QueryBox.SelectedText = tag.Tag.Name;
            }

            QueryBox.Select(QueryBox.Text.Length, 0);

        }

        private void QueryBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            FillTags(BooksApplication.Instance.Articles.Tags);
        }

        private string GetTagQuery()
        {
            var currentTag = GetCurrentTag();
            if (currentTag != null)
            {
                return currentTag.PlainText;
            }

            return "*";
        }

        private ITextRange GetCurrentTag()
        {
            var currentWord = GetCurrentWord();
            if (currentWord == null)
            {
                return null;
            }

            var allTags = new SearchQueryV2(QueryBox.Text).Tags;

            if (allTags.Contains(currentWord.PlainText.ToLowerInvariant()))
            {
                return GetCurrentWord();
            }

            return null;
        }

        private ITextRange GetCurrentWord()
        {
            if (string.IsNullOrWhiteSpace(QueryBox.Text))
            {
                return null;
            }

            var currentText = QueryBox.Text;
            var currentTextIndex = QueryBox.SelectionStart;
            var end = currentTextIndex;
            var endFound = false;

            for (int i = currentTextIndex; i < currentText.Length; i++)
            {
                if (TextParts.NotALetterOrNumber.Contains(currentText[i]))
                {
                    end = i - 1;
                    endFound = true;
                    break;
                }
            }

            if (endFound == false)
            {
                end = currentText.Length - 1;
            }

            var start = currentTextIndex;
            var startFound = false;

            for (int i = currentTextIndex - 1; i >= 0; i--)
            {
                if (TextParts.NotALetterOrNumber.Contains(currentText[i]))
                {
                    start = i + 1;
                    startFound = true;
                    break;
                }
            }

            if (startFound == false)
            {
                start = 0;
            }

            return new SubstringText(
                currentText,
                start,
                end - start + 1
            );
        }

        public void FillTags(Tags tags)
        {
            _tags = tags.Search(GetTagQuery(), clarifications: new SearchQueryV2(QueryBox.Text).Tags.And(_scope.Scope).ToArray())
                .Take(50).ToArray();

            Suggestions.Text = _tags
                .Select(x => $"{(x.Index > 0 && x.Index < 6 ? $"[{x.Index}] " : "")}{x.Tag.Name} ({x.Tag.Usage}) '{x.ClarificationScore}'")
                .JoinStringsWithoutSkipping(";\n");
        }

        public void Dispose()
        {
            _subscription.Dispose();
        }
    }
}