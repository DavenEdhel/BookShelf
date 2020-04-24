using System;
using System.Threading.Tasks;
using BookWiki.Core;
using BookWiki.Core.LifeSpellCheckModels;
using BookWiki.Core.Utils;
using BookWiki.Presentation.Apple.Controllers;
using BookWiki.Presentation.Apple.Models;
using BookWiki.Presentation.Apple.Models.HotKeys;
using BookWiki.Presentation.Apple.Models.Utils;
using BookWiki.Presentation.Apple.Views.Controls;
using CoreGraphics;
using Foundation;
using Keurig.IQ.Core.CrossCutting.Extensions;
using UIKit;

namespace BookWiki.Presentation.Apple.Views.Common
{
    public class EditTextView : UITextView, IKeyboardListener
    {
        private nint _cursorPosition;

        private readonly HotKeyScheme _viewModeScheme;
        private readonly HotKeyScheme _editModeScheme;
        private readonly LifeSpellCheck _lifeSpellCheck;

        public Func<NSMutableParagraphStyle> DefaultParagraph { get; set; }

        public event Action DidScrollEnd = delegate { };

        public EditTextView()
        {
            _lifeSpellCheck = new LifeSpellCheck(new TextViewErrorCollection(this), new CheckSpellingOperation());

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
            var leftTheLine2 = new HotKey(new Key(","), LeftCurrentLine).WithControl();
            var centerTheLine2 = new HotKey(new Key("."), CenterCurrentLine).WithControl();
            var rightTheLine2 = new HotKey(new Key("/"), RightCurrentLine).WithControl();
            var enableJoButton = new HotKey(new Key("]"), () => InsertText("ё"));
            var enableJoButtonUpperCase = new HotKey(new Key("]"), () => InsertText("Ё")).WithShift();
            var showSuggestions = new HotKey(new KeyCombination(new Key("z"), UIKeyModifierFlags.Control), ShowSuggestions);
            var addToDictionary = new HotKey(new KeyCombination(new Key("q"), UIKeyModifierFlags.Control), AddToDictionaryImmediately);
            var validate = new HotKey(new KeyCombination(new Key("r"), UIKeyModifierFlags.Control), CheckSpelling);

            _viewModeScheme = new HotKeyScheme(activateEditMode, validate);
            _editModeScheme = new HotKeyScheme(deactivateEditMode, leftTheLine, centerTheLine, rightTheLine, leftTheLine2, centerTheLine2, rightTheLine2, enableJoButton, enableJoButtonUpperCase, showSuggestions, validate, addToDictionary);

            DecelerationEnded += OnDecelerationEnded;
            WillEndDragging += OnWillEndDragging;
            ScrollAnimationEnded += OnScrollAnimationEnded;
        }

        public void ScrollTo(CGRect frame, bool animated = false)
        {
            if (frame.Y > ContentOffset.Y && frame.Y < (ContentOffset.Y + Frame.Height*0.9))
            {
                return;
            }

            var offset = frame.Y - Frame.Height/2;

            SetContentOffset(new CGPoint(0, offset > 0 ? offset : 0), animated);
        }

        private void OnScrollAnimationEnded(object sender, EventArgs e)
        {
            UpdateCurrentTextSpelling();

            DidScrollEnd();
        }

        private void OnWillEndDragging(object sender, WillEndDraggingEventArgs e)
        {
            if (Decelerating == false)
            {
                UpdateCurrentTextSpelling();

                DidScrollEnd();
            }
        }

        private void OnDecelerationEnded(object sender, EventArgs e)
        {
            UpdateCurrentTextSpelling();

            DidScrollEnd();
        }

        private void UpdateCurrentTextSpelling()
        {
            var start = GetClosestPositionToPoint(ContentOffset);

            var end = GetClosestPositionToPoint(new CGPoint(ContentOffset.X, ContentOffset.Y + Frame.Height));

            _lifeSpellCheck.TextChangedAround(((int)GetOffsetFromPosition(BeginningOfDocument, start) + (int)GetOffsetFromPosition(BeginningOfDocument, end)) / 2, Text);
        }

        private void OnChanged(object sender, EventArgs e)
        {
            _lifeSpellCheck.TextChangedAround((int)CursorPosition, Text);
        }

        public void CheckSpelling()
        {
            _lifeSpellCheck.ForceSpellChecking((int) CursorPosition, Text);
        }

        private void OnSelectionChanged(object sender, EventArgs e)
        {
            _cursorPosition = CursorPosition;

            _lifeSpellCheck.CursorPositionChanged((int)_cursorPosition, Text);
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

        public string SelectedText
        {
            get
            {
                var start = (int)GetOffsetFromPosition(BeginningOfDocument, SelectedTextRange.Start);
                var end = (int)GetOffsetFromPosition(BeginningOfDocument, SelectedTextRange.End);

                if (start == end)
                {
                    return string.Empty;
                }

                return Text.Substring(start, end - start);
            }
        }

        public async void Modify(Action<NSMutableAttributedString> modify)
        {
            var currentOffset = ContentOffset;

            var cursorPosition = CursorPosition;

            var result = new NSMutableAttributedString(AttributedText);

            modify(result);

            AttributedText = result;

            await Task.Delay(300);

            ContentOffset = currentOffset;

            CursorPosition = cursorPosition;
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
                new SuggestionsBoxView(spellChecker,
                        onChosen: ReplaceMisspelledWord,
                        onLearned: async () =>
                        {
                            await Task.Delay(100);

                            _lifeSpellCheck.ForceSpellChecking((int) CursorPosition, Text);
                        })
                    .Show();
            }

            async void ReplaceMisspelledWord(NSRange misspelledWord, string word)
            {
                var start = GetPosition(BeginningOfDocument, misspelledWord.Location);
                var end = GetPosition(start, misspelledWord.Length);

                var textRange = GetTextRange(start, end);

                ReplaceText(textRange, word);

                await Task.Delay(100);

                _lifeSpellCheck.MisspelledWordReplaced(new NativeRange(misspelledWord), Text);
            }
        }

        public async void AddToDictionaryImmediately()
        {
            var spellChecker = new SpellChecker(AttributedText.Value, (int)CursorPosition);

            spellChecker.Learn();

            await Task.Delay(100);

            _lifeSpellCheck.ForceSpellChecking((int) CursorPosition, Text);
        }

        public void LeftCurrentLine()
        {
            ChangeCurrentParagraphStyle(x => x.Alignment = UITextAlignment.Left);
        }

        public void RightCurrentLine() {
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
            UpdateCurrentTextSpelling();

            Application.Instance.RegisterSchemeForEditMode(_editModeScheme);
            Application.Instance.RegisterSchemeForViewMode(_viewModeScheme);
        }

        public void SelectTextRange(int nextFinding, int selectedTextLength)
        {
            SelectedRange = new NSRange(nextFinding, selectedTextLength);
        }
    }
}