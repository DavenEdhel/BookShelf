using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;
using BookWiki.Core;
using BookWiki.Core.LifeSpellCheckModels;
using BookWiki.Core.Utils;
using BookWiki.Core.Utils.TextModels;
using Keurig.IQ.Core.CrossCutting.Extensions;
using Inline = System.Windows.Documents.Inline;

namespace BookWiki.Presentation.Wpf.Models.SpellCheckModels
{
    public class LifeSpellCheckV2
    {
        private readonly FlowDocument _document;
        private readonly IErrorsCollectionV2 _errorsCollection;
        private readonly ISpellCheckerV2 _spellChecker;
        private int _lastCheckedIndex = 0;

        private int _lastCheckCursorPosition;

        public LifeSpellCheckV2(FlowDocument document, IErrorsCollectionV2 errorsCollection, ISpellCheckerV2 spellChecker)
        {
            _document = document;
            _errorsCollection = errorsCollection;
            _spellChecker = spellChecker;
        }

        public void TextChangedInside(Paragraph p)
        {
            if (p == null)
            {
                return;
            }

            _lastCheckCursorPosition = _document.ContentStart.GetOffsetToPosition(p.ContentStart);

            _errorsCollection.RemoveAll();

            var words = new PunctuationSeparatedEnumeration(_document, p).ToArray();

            foreach (var substring in words)
            {
                if (_spellChecker.IsCorrect(substring.Text) == false)
                {
                    _errorsCollection.Add(substring);
                }
            }

            _lastCheckedIndex = _lastCheckCursorPosition;
        }

        public void CursorPositionChanged()
        {

        }

        private Paragraph[] GetParagraphsInsideCursor()
        {
            return new Paragraph[0];
        }
    }

    public class RussianDictionarySpellChecker : ISpellCheckerV2
    {
        public bool IsCorrect(string word)
        {
            return false;
        }
    }

    public static class FlowDocumentExtensions
    {
        public static string GetText(this Inline inline)
        {
            switch (inline)
            {
                case Bold bold:
                    return GetText(bold.Inlines.FirstInline);
                case Italic italic:
                    return GetText(italic.Inlines.FirstInline);
                case Run run:
                    return run.Text;
                default:
                    throw new ArgumentOutOfRangeException(nameof(inline));
            }
        }
    }

    public class PunctuationSeparatedEnumeration : IEnumerable<ISubstring>
    {
        private readonly FlowDocument _document;
        private readonly Paragraph _p;

        public PunctuationSeparatedEnumeration(FlowDocument document, Paragraph p)
        {
            _document = document;
            _p = p;
        }

        public IEnumerator<ISubstring> GetEnumerator()
        {
            var result = new List<ISubstring>();

            var withDuplicates = GetIntervals().OrderBy(x => x.StartIndex).ToArray();

            foreach (var withDuplicate in withDuplicates)
            {
                if (result.Any(x => x.StartIndex == withDuplicate.StartIndex) == false)
                {
                    result.Add(withDuplicate);
                }
            }

            return result.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<ISubstring> GetIntervals()
        {
            var punctuation = TextParts.NotALetterOrNumber;

            // first index after word correction, move logic into core somehow (offset + text)

            int? end = null;
            string previousText = null;
            int previousOffset = 0;

            foreach (var i in _p.Inlines)
            {
                var offset = _document.ContentStart.GetOffsetToPosition(i.ContentStart);

                var text = i.GetText();

                var intervals = new IntervalsBetweenCharacters(text, punctuation).ToArray();

                if (intervals.Any())
                {
                    foreach (var interval in intervals)
                    {
                        if (end != null)
                        {
                            yield return new Substring(previousText.Substring(end.Value, previousText.Length - end.Value) + text.Substring(interval), previousOffset + end.Value, offset + interval.End + 1);

                            end = null;
                        }
                        else
                        {
                            yield return new Substring(text.Substring(interval), offset + interval.Start, offset + interval.End + 1);

                            end = null;
                        }
                    }

                    if (intervals.Max(x => x.End) == text.Length - 1)
                    {
                        end = intervals.Last().Start;
                        previousText = text;
                        previousOffset = offset;

                        if (i == _p.Inlines.LastInline)
                        {
                            yield return new Substring(previousText.Substring(end.Value, previousText.Length - end.Value), previousOffset + end.Value, previousOffset + previousText.Length);
                        }
                    }
                    else
                    {
                        end = null;
                    }
                }
                else
                {
                    if (end != null)
                    {
                        yield return new Substring(previousText.Substring(end.Value, previousText.Length - end.Value), previousOffset + end.Value, previousOffset + previousText.Length);

                        end = null;
                    }
                }
            }
        }
    }
}