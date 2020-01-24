using System;
using System.Net.Mime;
using BookWiki.Core.Utils;
using BookWiki.Core.Utils.TextModels;

namespace BookWiki.Core.LifeSpellCheckModels
{
    public class LifeSpellCheck
    {
        private readonly IErrorsCollection _errorsCollection;
        private readonly ISpellChecker _spellChecker;
        private const int HalfOfRange = 1000;
        private string _lastCheckedText = string.Empty;
        private int _lastCheckedIndex = 0;

        private int _lastCheckCursorPosition;

        public LifeSpellCheck(IErrorsCollection errorsCollection, ISpellChecker spellChecker)
        {
            _errorsCollection = errorsCollection;
            _spellChecker = spellChecker;
        }

        public void TextChangedAround(int index, string newText)
        {
            if (IsNeedToCheck(index, newText) == false)
            {
                return;
            }

            _lastCheckCursorPosition = index;

            _errorsCollection.RemoveAll();

            var misspelledWords = _spellChecker.Execute(newText, new PunctuationSeparatedRange(newText, index - HalfOfRange, 2*HalfOfRange));

            foreach (var misspelledWord in misspelledWords)
            {
                _errorsCollection.Add(misspelledWord);
            }

            _lastCheckedIndex = index;
            _lastCheckedText = newText;
        }

        public void MisspelledWordReplaced(IRange oldWord, string newText)
        {
            TextChangedAround(oldWord.Offset, newText);
        }

        public void ForceSpellChecking(int cursorPosition, string text)
        {
            TextChangedAround(cursorPosition, text);
        }

        public void CursorPositionChanged(int newCursorPosition, string text)
        {
            if (Math.Abs(_lastCheckCursorPosition - newCursorPosition) > HalfOfRange)
            {
                TextChangedAround(newCursorPosition, text);
            }
        }

        private bool IsNeedToCheck(int index, string newText)
        {
            if (newText != _lastCheckedText)
            {
                return true;
            }

            if (Math.Abs(index - _lastCheckedIndex) > HalfOfRange)
            {
                return true;
            }

            return false;
        }
    }
}