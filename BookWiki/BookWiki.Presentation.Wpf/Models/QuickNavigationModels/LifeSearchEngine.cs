using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using BookWiki.Core.Articles;
using BookWiki.Core.LifeSpellCheckModels;
using BookWiki.Core.Utils.TextModels;
using BookWiki.Presentation.Wpf.Models.SpellCheckModels;
using Keurig.IQ.Core.CrossCutting.Extensions;

namespace BookWiki.Presentation.Wpf.Models.QuickNavigationModels
{
    public class LifeSearchEngine
    {
        private FlowDocument _document;
        private RichTextBox _rtb;
        private readonly IHighlightCollection _highlightCollection;
        private readonly TextBox _searchBox;
        private readonly ScrollViewer _scroll;
        private ISubstring _lastFound;
        private Paragraph _lastCheckedParagraph;

        public LifeSearchEngine(RichTextBox rtb, IHighlightCollection highlightCollection, TextBox searchBox, ScrollViewer scroll)
        {
            _document = rtb.Document;
            _rtb = rtb;
            _highlightCollection = highlightCollection;
            _searchBox = searchBox;
            _scroll = scroll;

            _searchBox.KeyDown += SearchBoxOnKeyDown;
            _searchBox.TextChanged += SearchBoxOnTextChanged;

            _searchBox.LostFocus += SearchBoxOnLostFocus;

            _scroll.ScrollChanged += ScrollOnScrollChanged;
        }

        private void ScrollOnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            HighlightVisible(_lastFound);
        }

        private void SearchBoxOnTextChanged(object sender, TextChangedEventArgs e)
        {
            HighlightVisible(null);
            _lastFound = null;
            _lastCheckedParagraph = null;
        }

        private void SearchBoxOnLostFocus(object sender, RoutedEventArgs e)
        {
            _highlightCollection.ClearHighlighting();
            _lastFound = null;
            _lastCheckedParagraph = null;
        }

        private void SearchBoxOnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ScrollToNext();
                return;
            }

            if (e.Key == Key.Escape)
            {
                _lastFound = null;
                _highlightCollection.ClearHighlighting();
                _rtb.Document.Focus();
                return;
            }
        }

        private void ScrollToNext()
        {
            var next = FindNext();

            if (next == null)
            {
                return;
            }

            HighlightVisible(next);

            _highlightCollection.ScrollTo(next);
        }

        private ISubstring FindNext()
        {
            foreach (var p in GetAllVisibleParagraphsStartingFrom(_lastCheckedParagraph))
            {
                var words = new PunctuationSeparatedEnumeration(_document, p).ToArray();

                foreach (var substring in words)
                {
                    if (_lastFound != null)
                    {
                        if (_lastFound.StartIndex >= substring.StartIndex)
                        {
                            continue;
                        }
                    }

                    if (substring.Text.ToUpperInvariant().Contains(_searchBox.Text.ToUpperInvariant()))
                    {
                        _lastFound = substring;
                        _lastCheckedParagraph = p;

                        return substring;
                    }
                }

                _lastFound = null;
            }

            return null;
        }

        private void HighlightVisible(ISubstring specialString)
        {
            if (_searchBox.IsFocused == false)
            {
                return;
            }

            if (_searchBox.Text.Length < 4)
            {
                return;
            }

            _highlightCollection.ClearHighlighting();

            foreach (var p in GetAllVisibleParagraphs())
            {
                var words = new PunctuationSeparatedEnumeration(_document, p).ToArray();

                foreach (var substring in words)
                {
                    if (substring.Text.ToUpperInvariant().Contains(_searchBox.Text.ToUpperInvariant()))
                    {
                        _highlightCollection.Highlight(substring, substring.StartIndex == specialString?.StartIndex);
                    }
                }
            }
        }

        private IEnumerable<Paragraph> GetAllVisibleParagraphs()
        {
            var first = _rtb.GetPositionFromPoint(new Point(0, _scroll.VerticalOffset), true).Paragraph;
            var last = _rtb.GetPositionFromPoint(new Point(0, _scroll.VerticalOffset + _scroll.ActualHeight), true).Paragraph;
            var current = first;

            yield return current;

            while (current != last)
            {
                current = current.NextBlock?.CastTo<Paragraph>();

                if (current != null)
                {
                    yield return current;
                }
            }
        }

        private IEnumerable<Paragraph> GetAllVisibleParagraphsStartingFrom(Paragraph p)
        {
            var first = p ?? _rtb.Document.Blocks.FirstBlock.CastTo<Paragraph>();
            var current = first;

            yield return current;

            while (current.NextBlock != null)
            {
                current = current.NextBlock.CastTo<Paragraph>();

                yield return current;
            }

            current = _rtb.Document.Blocks.FirstBlock.CastTo<Paragraph>();

            while (current != first)
            {
                yield return current;

                current = current.NextBlock.CastTo<Paragraph>();
            }
        }
    }

    public class NavigateToArticleEngine
    {
        private readonly SpecialItemsHighlightEngine _specialItemsHighlighter;

        public NavigateToArticleEngine(RichTextBox rtb,
            IHighlightCollection highlightCollection,
            ScrollViewer scroll)
        {
            _specialItemsHighlighter = new SpecialItemsHighlightEngine(rtb, highlightCollection, BookShelf.Instance.Articles, scroll,
                specialItem =>
                {
                    var searchResult = BookShelf.Instance.Articles.Search($"!{specialItem.Trim().ToLowerInvariant()}");

                    if (searchResult.Count() == 1)
                    {
                        BookShelf.Instance.OpenArticle(searchResult.First().Source);
                    }
                    else
                    {
                        BookShelf.Instance.OpenArticlesSearch($"!{specialItem.Trim().ToLowerInvariant()}");
                    }
                });
        }

        public bool IsApplicable(ISubstring selectedSubstring)
        {
            return _specialItemsHighlighter.IsApplicable(selectedSubstring);
        }

        public void Navigate(ISubstring substring)
        {
            _specialItemsHighlighter.Navigate(substring);
        }
    }

    public class SpecialItemsHighlightEngine
    {
        private FlowDocument _document;
        private RichTextBox _rtb;
        private readonly IHighlightCollection _highlightCollection;
        private readonly ArticlesLibrary _articles;

        private List<string> SpecialItems
        {
            get
            {
                return _articles.Search("*").SelectMany(x => x.NameVariations).Select(x => x.ToLowerInvariant()).ToList();
            }
        }

        private readonly ScrollViewer _scroll;
        private readonly Action<string> _onSpecialItemClick;
        private bool highlighted;

        public SpecialItemsHighlightEngine(
            RichTextBox rtb,
            IHighlightCollection highlightCollection,
            ArticlesLibrary articles,
            ScrollViewer scroll,
            Action<string> onSpecialItemClick)
        {
            _document = rtb.Document;
            _rtb = rtb;
            _highlightCollection = highlightCollection;
            _articles = articles;
            _scroll = scroll;
            _onSpecialItemClick = onSpecialItemClick;

            _rtb.PreviewKeyDown += RtbOnPreviewKeyDown;
            _rtb.PreviewKeyUp += RtbOnPreviewKeyUp;
        }

        private void RtbOnPreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (highlighted)
            {
                _highlightCollection.ClearHighlighting();
                highlighted = false;
            }
        }

        private void RtbOnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Shift) &&
                e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control))
            {
                HighlightAll();
                highlighted = true;
            }
            
        }

        private void HighlightAll()
        {
            _highlightCollection.ClearHighlighting();

            foreach (var p in GetAllVisibleParagraphs())
            {
                var words = new PunctuationSeparatedEnumeration(_document, p).ToArray();

                foreach (var substring in words)
                {
                    if (SpecialItems.Contains(substring.Text.ToLowerInvariant()))
                    {
                        _highlightCollection.Highlight(substring, false,
                            text =>
                            {
                                _highlightCollection.ClearHighlighting();

                                _onSpecialItemClick(text);
                            });
                    }
                }
            }
        }

        private IEnumerable<Paragraph> GetAllVisibleParagraphs()
        {
            var first = _rtb.GetPositionFromPoint(new Point(0, _scroll.VerticalOffset), true).Paragraph;
            var last = _rtb.GetPositionFromPoint(new Point(0, _scroll.VerticalOffset + _scroll.ActualHeight), true).Paragraph;
            var current = first;

            yield return current;

            while (current != last)
            {
                current = current.NextBlock?.CastTo<Paragraph>();

                if (current != null)
                {
                    yield return current;
                }
            }
        }

        public bool IsApplicable(ISubstring selectedSubstring)
        {
            return SpecialItems.Contains(selectedSubstring.Text.ToLowerInvariant());
        }

        public void Navigate(ISubstring substring)
        {
            _onSpecialItemClick(substring.Text.ToLowerInvariant());
        }
    }
}