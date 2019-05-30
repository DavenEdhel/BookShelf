using System.Collections;
using System.Collections.Generic;
using BookWiki.Core;
using BookWiki.Core.Utils;
using Foundation;
using UIKit;

namespace BookWiki.Presentation.Apple.Views.Controls
{
    public class MisspelledWordsSequence : ISequence<NSRange>
    {
        private readonly UITextChecker _textChecker;
        private readonly string _plainText;

        private SpaceSeparatedRange _range;

        public MisspelledWordsSequence(UITextChecker textChecker, string plainText, int startIndex = 0, int length = -1) : this(textChecker, plainText, new SpaceSeparatedRange(plainText, startIndex, length))
        {
        }

        public MisspelledWordsSequence(UITextChecker textChecker, string plainText, SpaceSeparatedRange range)
        {
            _textChecker = textChecker;
            _plainText = plainText;

            _range = range;

            Completion = new Progress(plainText.Length);
        }

        public IEnumerator<NSRange> GetEnumerator()
        {
            var position = _range.StartIndex;

            Completion.Change(position);

            while (position < _range.EndIndex)
            {
                var misspelled = _textChecker.RangeOfMisspelledWordInString(_plainText,
                    new NSRange(0, _plainText.Length), position, false, "ru_RU");

                if (misspelled.Location == NSRange.NotFound)
                {
                    Completion.MarkCompleted();

                    yield break;
                }

                yield return misspelled;

                position = (int)misspelled.Location + (int)misspelled.Length + 1;

                Completion.Change(position);
            }

            Completion.MarkCompleted();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Progress Completion { get; }
    }
}