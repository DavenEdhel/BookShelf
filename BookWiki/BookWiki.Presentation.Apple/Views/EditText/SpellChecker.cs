using System.Collections.Generic;
using System.Linq;
using BookWiki.Core;
using BookWiki.Core.Utils.TextModels;
using BookWiki.Presentation.Apple.Models.Utils;
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

            MisspelledWords = new RunOnceSequence<IRange>(new MisspelledWordsSequence(_textChecker, plainText));
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

        public ISequence<IRange> MisspelledWords { get; }

        public bool IsCursorInMisspelledWord => MisspelledWords.Any(TestCursorInRange);

        public IEnumerable<string> Guesses
        {
            get
            {
                if (IsCursorInMisspelledWord)
                {
                    var misspelledWord = MisspelledWord;

                    return _textChecker.GuessesForWordRange(misspelledWord.ToNsRange(), _plainText, "ru_RU").Take(12);
                }

                return Enumerable.Empty<string>();
            }
        }

        public IRange MisspelledWord => MisspelledWords.First(TestCursorInRange);

        private bool TestCursorInRange(IRange word) => _cursorPosition >= word.Offset && _cursorPosition <= (word.Offset + word.Length);

        public void Learn()
        {
            var word = MisspelledWord;

            UITextChecker.LearnWord(_plainText.Substring((int)word.Offset, (int)word.Length).Trim());
        }
    }
}