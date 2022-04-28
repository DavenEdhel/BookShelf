using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Documents;
using BookWiki.Core;
using BookWiki.Core.LifeSpellCheckModels;
using BookWiki.Core.Logging;
using BookWiki.Core.Utils;
using Keurig.IQ.Core.CrossCutting.Extensions;

namespace BookWiki.Presentation.Wpf.Models.SpellCheckModels
{
    public class LifeSpellCheckV2
    {
        private readonly FlowDocument _document;
        private readonly RichTextBox _rtb;
        private readonly IErrorsCollectionV2 _errorsCollection;
        private readonly ISpellCheckerV2 _spellChecker;
        private int _lastCheckedIndex = Int32.MinValue/2;
        private const int HalfOfRange = 1000;
        private Logger _logger = new Logger(nameof(LifeSpellCheckV2));
        private bool _skipCursorCheck = false;
        private bool _isEnabled = true;

        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                _isEnabled = value;

                if (_isEnabled == false)
                {
                    _lastCheckedIndex = Int32.MinValue / 2;
                    _skipCursorCheck = false;

                    _errorsCollection.RemoveAll();
                }
            }
        }

        public LifeSpellCheckV2(RichTextBox rtb, IErrorsCollectionV2 errorsCollection, ISpellCheckerV2 spellChecker)
        {
            _document = rtb.Document;
            _rtb = rtb;
            _errorsCollection = errorsCollection;
            _spellChecker = spellChecker;
        }

        public void TextChangedInside(Paragraph p)
        {
            if (IsEnabled == false)
            {
                return;
            }

            if (p == null)
            {
                return;
            }

            _logger.Info("Text changed. Cursor check invalidated");

            _errorsCollection.RemoveAll();

            CheckSpelling(p);

            _skipCursorCheck = true;

            _lastCheckedIndex = Int32.MinValue/2;
        }

        public void CursorPositionChanged()
        {
            if (IsEnabled == false)
            {
                return;
            }

            if (_skipCursorCheck)
            {
                _logger.Info("Cursor check skipped");

                _skipCursorCheck = false;
                return;
            }

            _logger.Info("Cursor position changed");

            var caretOffset = _rtb.Document.ContentStart.GetOffsetToPosition(_rtb.CaretPosition);

            if (Math.Abs(caretOffset - _lastCheckedIndex) > HalfOfRange)
            {
                _logger.Info("Recheck spelling");

                _errorsCollection.RemoveAll();

                foreach (var paragraph in GetParagraphsInsideCursor())
                {
                    CheckSpelling(paragraph);
                }

                _lastCheckedIndex = caretOffset;
            }
        }

        private void CheckSpelling(Paragraph p)
        {
            var words = new PunctuationSeparatedEnumeration(_document, p).ToArray();

            foreach (var substring in words)
            {
                if (_spellChecker.IsCorrect(substring.Text) == false)
                {
                    _errorsCollection.Add(substring);
                }
            }
        }

        private IEnumerable<Paragraph> GetParagraphsInsideCursor()
        {
            var caretOffset = _rtb.Document.ContentStart.GetOffsetToPosition(_rtb.CaretPosition);

            foreach (var block in _rtb.Document.Blocks.Where(x => x is Paragraph))
            {
                var pOffset = _rtb.Document.ContentStart.GetOffsetToPosition(block.ContentStart);

                if (Math.Abs(pOffset - caretOffset) < HalfOfRange)
                {
                    yield return block.CastTo<Paragraph>();
                }
            }
        }

        public void Invalidate()
        {
            _lastCheckedIndex = Int32.MinValue / 2;
            CursorPositionChanged();
        }
    }
}