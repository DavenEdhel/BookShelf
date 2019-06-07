using System;
using System.Collections.Generic;
using System.Linq;
using BookWiki.Core.Utils;
using BookWiki.Core.Utils.TextModels;
using BookWiki.Presentation.Apple.Models.Utils;
using BookWiki.Presentation.Apple.Views.Controls;
using CoreGraphics;
using Foundation;
using UIKit;

namespace BookWiki.Presentation.Apple.Views.Common
{
    public class EditTextView : UITextView
    {
        private bool _changed;
        private nint _cursorPosition;
        private bool _positionIsSet;

        public bool WasChanged => _changed;

        private readonly List<ErrorLineView> _errors = new List<ErrorLineView>();

        public Func<NSMutableParagraphStyle> DefaultParagraph { get; set; }

        public EditTextView()
        {
            SelectionChanged += OnSelectionChanged;
            DefaultParagraph = () => new NSMutableParagraphStyle();
        }

        public void ReplaceMisspelledWord(NSRange misspelledWord, string word)
        {
            var start = GetPosition(BeginningOfDocument, misspelledWord.Location);
            var end = GetPosition(start, misspelledWord.Length);

            var textRange = GetTextRange(start, end);

            Modify(result =>
            {
                ReplaceText(textRange, word);
            });

            MarkSpellingForCursorRange();
        }

        public void CheckSpelling()
        {
            MarkRange(new Range(Text.Length, 0), new MisspelledWordsSequence(new UITextChecker(), Text));
        }

        public void MarkSpellingForCursorRange()
        {
            var range = new SpaceSeparatedRange(Text, (int)CursorPosition - 50, 100);

            MarkRange(range, new MisspelledWordsSequence(new UITextChecker(), Text, range));
        }

        private void MarkRange(IRange scope, IEnumerable<NSRange> errors)
        {
            var errorsToRemove = _errors.Where(x => x.In(scope)).ToArray();

            foreach (var errorLineView in errorsToRemove)
            {
                errorLineView.RemoveFromSuperview();
                _errors.Remove(errorLineView);
            }

            foreach (var nsRange in errors)
            {
                var e = new ErrorLineView(nsRange, this);
                _errors.Add(e);
            }
        }

        private void OnSelectionChanged(object sender, EventArgs e)
        {
            _positionIsSet = true;
            _cursorPosition = CursorPosition;
        }

        public nint CursorPosition
        {
            get => GetOffsetFromPosition(BeginningOfDocument, SelectedTextRange.End);
            set
            {
                var cursorPosition = GetPosition(BeginningOfDocument, value);

                cursorPosition = cursorPosition ?? GetPosition(BeginningOfDocument, 0);

                SelectedTextRange = GetTextRange(cursorPosition, cursorPosition);
            }
        }

        public CGPoint CursorLocation
        {
            get
            {
                var cursorPosition = GetPosition(BeginningOfDocument, CursorPosition);

                var cursorFrame = GetCaretRectForPosition(cursorPosition);

                return cursorFrame.Location;
            }
        }

        public void Modify(Action<NSMutableAttributedString> modify)
        {
            var currentOffset = ContentOffset;

            var caret = SelectedTextRange;

            var result = new NSMutableAttributedString(AttributedText);

            modify(result);

            AttributedText = result;

            _changed = true;

            ContentOffset = currentOffset;

            SelectedTextRange = caret;
        }

        public void ChangeCurrentParagraphStyle(Action<NSMutableParagraphStyle> changeParagraphStyle)
        {
            Modify(result =>
            {
                var p = SelectedParagraph;

                result.RemoveAttribute(UIStringAttributeKey.ParagraphStyle, p);

                var paragraph = DefaultParagraph();

                changeParagraphStyle(paragraph);

                result.AddAttribute(UIStringAttributeKey.ParagraphStyle, paragraph, p);
            });
        }

        public void ActivateEditMode()
        {
            BecomeFirstResponder();

            var cursorPosition = GetPosition(BeginningOfDocument, _cursorPosition);

            SelectedTextRange = GetTextRange(cursorPosition, cursorPosition);
        }

        public void DeactivateEditMode()
        {
            if (Focused)
            {
                _cursorPosition = CursorPosition;
            }

            ResignFirstResponder();
        }

        private NSRange SelectedParagraph
        {
            get
            {
                var selectedRange = SelectedRange;

                var start = (int)selectedRange.Location;
                var end = (int)selectedRange.Location;

                var text = Text;

                start = GetStart();

                for (int i = end; i < text.Length; i++)
                {
                    if (text[i] == '\n')
                    {
                        end = i;
                        break;
                    }

                    end = i;
                }

                return new NSRange(start, end - start);

                int GetStart()
                {
                    for (int i = new Number(start, 0, text.Length - 1); i > 0; i--)
                    {
                        if (text[i] == '\n')
                        {
                            return i + 1;
                        }
                    }

                    return 0;
                }
            }
        }
    }
}