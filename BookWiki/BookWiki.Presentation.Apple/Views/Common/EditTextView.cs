using System;
using System.Collections.Generic;
using System.Linq;
using BookWiki.Core;
using BookWiki.Core.Utils;
using BookWiki.Core.Utils.TextModels;
using BookWiki.Presentation.Apple.Controllers;
using BookWiki.Presentation.Apple.Models;
using BookWiki.Presentation.Apple.Models.HotKeys;
using BookWiki.Presentation.Apple.Models.Utils;
using BookWiki.Presentation.Apple.Views.Controls;
using CoreGraphics;
using Foundation;
using UIKit;

namespace BookWiki.Presentation.Apple.Views.Common
{
    public class EditTextView : UITextView, IKeyboardListener
    {
        private bool _changed;
        private nint _cursorPosition;
        private bool _positionIsSet;

        public bool WasChanged => _changed;

        private readonly List<ErrorLineView> _errors = new List<ErrorLineView>();

        private readonly HotKeyScheme _viewModeScheme;
        private readonly HotKeyScheme _editModeScheme;

        public Func<NSMutableParagraphStyle> DefaultParagraph { get; set; }

        public EditTextView()
        {
            SelectionChanged += OnSelectionChanged;
            Changed += OnChanged;
            DefaultParagraph = () => new NSMutableParagraphStyle();

            Editable = true;
            AllowsEditingTextAttributes = true;
            AutocorrectionType = UITextAutocorrectionType.No;
            AutocapitalizationType = UITextAutocapitalizationType.None;
            ShowsVerticalScrollIndicator = false;
            Font = UIFont.FromName("TimesNewRomanPSMT", 20);
            ContentInset = new UIEdgeInsets(0, 0, 50, 0);

            var deactivateEditMode = new HotKey(Key.Escape, DeactivateEditMode);
            var activateEditMode = new HotKey(Key.Enter, ActivateEditMode);
            var leftTheLine = new HotKey(new Key("7"), LeftCurrentLine).WithCommand();
            var centerTheLine = new HotKey(new Key("8"), CenterCurrentLine).WithCommand();
            var rightTheLine = new HotKey(new Key("9"), RightCurrentLine).WithCommand();
            var enableJoButton = new HotKey(new Key("]"), () => InsertText("ё"));
            var enableJoButtonUpperCase = new HotKey(new Key("]"), () => InsertText("Ё")).WithShift();
            var showSuggestions = new HotKey(new KeyCombination(Key.Space, UIKeyModifierFlags.Control), ShowSuggestions);
            var validate = new HotKey(new KeyCombination(new Key("r"), UIKeyModifierFlags.Control), CheckSpelling);

            _viewModeScheme = new HotKeyScheme(activateEditMode, validate);
            _editModeScheme = new HotKeyScheme(deactivateEditMode, leftTheLine, centerTheLine, rightTheLine, enableJoButton, enableJoButtonUpperCase, showSuggestions, validate);
        }

        private void OnChanged(object sender, EventArgs e)
        {
            MarkSpellingForCursorRange();
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
            MarkRange(new Range((int)AttributedText.Length, 0), new MisspelledWordsSequence(new UITextChecker(), AttributedText.Value));
        }

        public void MarkSpellingForCursorRange()
        {
            var range = new SpaceSeparatedRange(AttributedText.Value, (int)CursorPosition - 50, 100);

            MarkRange(range, new MisspelledWordsSequence(new UITextChecker(), AttributedText.Value, range));
        }

        private void MarkRange(IRange scope, IEnumerable<NSRange> errors)
        {
            var newErrorRanges = errors.Select(x => new NativeRange(x)).ToArray();

            var displayedErrorsInScope = _errors.Where(x => x.In(scope)).ToArray();

            var displayedErrorsToRemove = displayedErrorsInScope.Where(x => newErrorRanges.Any(n => x.Exact(n)) == false).ToArray();
            var errorsToAdd = newErrorRanges.Where(x => displayedErrorsInScope.Any(e => e.Exact(x)) == false).ToArray();

            foreach (var errorLineView in displayedErrorsToRemove)
            {
                errorLineView.RemoveFromSuperview();
                _errors.Remove(errorLineView);
            }

            foreach (var range in errorsToAdd)
            {
                var e = new ErrorLineView(range, this);
                _errors.Add(e);
            }
        }

        private void ShowRanges()
        {
            foreach (var errorLineView in _errors)
            {
                errorLineView.DumpToConsole();
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

        public void ScrollDown()
        {
            var delta = Frame.Height / 2;

            SetContentOffset(new CGPoint(0, ContentOffset.Y + delta), true);
        }

        public void ScrollUp()
        {
            var delta = Frame.Height / 2;

            SetContentOffset(new CGPoint(0, ContentOffset.Y - delta), true);
        }

        public void ShowSuggestions()
        {
            var spellChecker = new SpellChecker(AttributedText.Value, (int)CursorPosition);

            if (spellChecker.IsCursorInMisspelledWord)
            {
                new SuggestionsBoxView(spellChecker, ReplaceMisspelledWord, CheckSpelling).Show();
            }
        }

        public void LeftCurrentLine()
        {
            ChangeCurrentParagraphStyle(x => x.Alignment = UITextAlignment.Left);
        }

        public void RightCurrentLine()
        {
            ChangeCurrentParagraphStyle(x => x.Alignment = UITextAlignment.Right);
        }

        public void CenterCurrentLine()
        {
            ChangeCurrentParagraphStyle(x => x.Alignment = UITextAlignment.Center);
        }

        public void Pause()
        {
            Application.Instance.UnregisterScheme(_editModeScheme);
            Application.Instance.UnregisterScheme(_viewModeScheme);
        }

        public void Resume()
        {
            Application.Instance.RegisterSchemeForEditMode(_editModeScheme);
            Application.Instance.RegisterSchemeForViewMode(_viewModeScheme);
        }

        
    }
}