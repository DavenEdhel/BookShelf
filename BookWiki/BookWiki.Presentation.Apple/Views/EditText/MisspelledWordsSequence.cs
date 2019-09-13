using System.Collections;
using System.Collections.Generic;
using BookWiki.Core;
using BookWiki.Core.Utils;
using BookWiki.Core.Utils.TextModels;
using BookWiki.Presentation.Apple.Models.Utils;
using Foundation;
using UIKit;

namespace BookWiki.Presentation.Apple.Views.Controls
{
    public class MisspelledWordsSequence : ISequence<IRange>
    {
        private readonly UITextChecker _textChecker;
        private readonly string _plainText;
        private readonly IRange _range;

        public MisspelledWordsSequence(UITextChecker textChecker, string plainText, int startIndex = 0, int length = -1) : this(textChecker, plainText, new SpaceSeparatedRange(plainText, startIndex, length))
        {
        }

        public MisspelledWordsSequence(UITextChecker textChecker, string plainText, IRange range)
        {
            _textChecker = textChecker;
            _plainText = plainText;

            _range = range;

            Completion = new Progress(plainText.Length);
        }

        public IEnumerator<IRange> GetEnumerator()
        {
            var position = _range.Start();

            Completion.Change(position);

            while (position < _range.End())
            {
                var misspelled = _textChecker.RangeOfMisspelledWordInString(_plainText,
                    new NSRange(_range.Start(), _range.Length), position, false, "ru_RU");

                if (misspelled.Location == NSRange.NotFound)
                {
                    Completion.MarkCompleted();

                    yield break;
                }

                yield return new NativeRange(misspelled);

                position = (int)misspelled.Location + (int)misspelled.Length;

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