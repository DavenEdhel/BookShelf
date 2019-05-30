using System.Collections.Generic;
using System.Linq;
using BookWiki.Core;
using Foundation;
using Keurig.IQ.Core.CrossCutting.Extensions;
using UIKit;

namespace BookWiki.Presentation.Apple.Views.Controls
{
    public class SpellChecker
    {
        // https://nshipster.com/uitextchecker/

        private readonly string _plainText;
        private readonly int _cursorPosition;
        private readonly UITextChecker _textChecker;

        public SpellChecker(string plainText, int cursorPosition = 0)
        {
            _plainText = plainText;
            _cursorPosition = Normalize(plainText, cursorPosition);
            _textChecker = new UITextChecker();

            MisspelledWords = new RunOnceSequence<NSRange>(new MisspelledWordsSequence(_textChecker, plainText));
        }

        private int Normalize(string plainText, int cursor)
        {
            if (cursor == 0)
            {
                return 0;
            }

            cursor--;

            while (char.IsLetter(plainText[cursor]) == false)
            {
                cursor--;
            }

            return cursor;
        }

        public ISequence<NSRange> MisspelledWords { get; }

        public bool IsCursorInMisspelledWord
        {
            get
            {
                return MisspelledWords.Any(TestCursorInRange);
            }
        }

        public IEnumerable<string> Guesses
        {
            get
            {
                if (IsCursorInMisspelledWord)
                {
                    var misspelledWord = MisspelledWord;

                    return _textChecker.GuessesForWordRange(misspelledWord, _plainText, "ru_RU");
                }

                return Enumerable.Empty<string>();
            }
        }

        public NSRange MisspelledWord => MisspelledWords.First(TestCursorInRange);

        private bool TestCursorInRange(NSRange word) => _cursorPosition >= word.Location && _cursorPosition <= (word.Location + word.Length);

        public void Learn()
        {
            var word = MisspelledWord;

            UITextChecker.LearnWord(_plainText.Substring((int)word.Location, (int)word.Length).Trim());
        }
    }
}