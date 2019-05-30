using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BookWiki.Core;
using BookWiki.Core.Files.PathModels;
using BookWiki.Core.FileSystem.PathModels;
using BookWiki.Core.Utils;
using BookWiki.Presentation.Apple.Controllers;
using BookWiki.Presentation.Apple.Extentions;
using BookWiki.Presentation.Apple.Models;
using BookWiki.Presentation.Apple.Models.HotKeys;
using BookWiki.Presentation.Apple.Models.Utils;
using CoreFoundation;
using CoreGraphics;
using CoreText;
using Foundation;
using UIKit;

namespace BookWiki.Presentation.Apple.Views.Controls
{
    public class NovelView : View, IContentView, INovel, IKeyboardListener
    {
        private readonly INovel _novel;
        private readonly ILibrary _library;

        private bool _wasFocused;
        private nint _cursorPosition;
        private HotKeyScheme _viewModeScheme;
        private HotKeyScheme _editModeScheme;
        private UITextView _content;
        private int _margin = 24;

        private bool _changed = false;
        private IPath _source;

        private CursorView _cursor;

        private DeferredAction _save;

        public NovelView(INovel novel, ILibrary library)
        {
            _novel = novel;
            _library = library;

            var deactivateEditMode = new HotKey(Key.Escape, DeactivateEditMode);
            var activateEditMode = new HotKey(Key.Enter, ActivateEditMode);
            var leftTheLine = new HotKey(new Key("7"), LeftCurrentLine).WithCommand();
            var centerTheLine = new HotKey(new Key("8"), CenterCurrentLine).WithCommand();
            var rightTheLine = new HotKey(new Key("9"), RightCurrentLine).WithCommand();
            var enableJoButton = new HotKey(new Key("]"), EnableJoButton);
            var enableJoButtonUpperCase = new HotKey(new Key("]"), EnableJoButtonUpperCase).WithShift();
            var showAutocorrection = new HotKey(new KeyCombination(Key.Space, UIKeyModifierFlags.Shift), ShowAutocorrection);
            //var moveCursorToRight = new HotKey(new Key(";"), MoveCursorToRight);
            //var moveCursorToLeft = new HotKey(new Key("l"), MoveCursorToLeft);

            _viewModeScheme = new HotKeyScheme(activateEditMode);
            _editModeScheme = new HotKeyScheme(deactivateEditMode, leftTheLine, centerTheLine, rightTheLine, enableJoButton, enableJoButtonUpperCase, showAutocorrection);

            _save = new DeferredAction(TimeSpan.FromSeconds(10), () => _library.Update(this));

            Initialize();
        }

        private void ShowAutocorrection()
        {
            var spellChecker = new SpellChecker(_content.Text, (int)CursorPosition);

            if (spellChecker.IsCursorInMisspelledWord)
            {
                var box = new AutocorectionBoxView(spellChecker, ReplaceOnCurrentCursorWith);

                box.Show(this, CursorLocation.Move(30, 30));
            }
        }

        private void ReplaceOnCurrentCursorWith(NSRange misspelledWord, string word)
        {
            var start = _content.GetPosition(_content.BeginningOfDocument, misspelledWord.Location);
            var end = _content.GetPosition(start, misspelledWord.Length);

            var textRange = _content.GetTextRange(start, end);

            _content.ReplaceText(textRange, word);

            MarkSpellingForCursorRange();
        }

        private void EnableJoButtonUpperCase()
        {
            _content.InsertText("Ё");
        }

        private void EnableJoButton()
        {
            _content.InsertText("ё");
        }

        private void InstanceOnModeChanged(bool obj)
        {

        }

        private void InitCursor()
        {
            //Add(_cursor);
        }

        private void ActivateEditMode()
        {
            _content.BecomeFirstResponder();

            CursorPosition = _virtualCursorPosition;
        }

        private void DeactivateEditMode()
        {
            _content.ResignFirstResponder();
        }

        private int _virtualCursorPosition = 10;
        private Timer _timer;

        private void MoveCursorToRight()
        {
            _virtualCursorPosition += 10;

            var cursorPosition = _content.GetPosition(_content.BeginningOfDocument, _virtualCursorPosition);

            var cursorFrame = _content.GetCaretRectForPosition(cursorPosition);

            _cursor.ChangePosition(cursorFrame.X, cursorFrame.Y);
        }

        private void MoveCursorToLeft()
        {
            _virtualCursorPosition -= 10;

            var cursorPosition = _content.GetPosition(_content.BeginningOfDocument, _virtualCursorPosition);

            var cursorFrame = _content.GetCaretRectForPosition(cursorPosition);

            _cursor.ChangePosition(cursorFrame.X, cursorFrame.Y);
        }

        private void Initialize()
        {
            _content = new UITextView();

            _content.Editable = true;
            _content.AllowsEditingTextAttributes = true;
            _content.ShouldChangeText += OnShouldChangeText;
            _content.Changed += ContentOnChanged;
            _content.AutocorrectionType = UITextAutocorrectionType.No;
            _content.AutocapitalizationType = UITextAutocapitalizationType.None;
            _content.SelectionChanged += OnSelectionChanged;
            _content.ShowsVerticalScrollIndicator = false;
            _content.Font = UIFont.FromName("TimesNewRomanPSMT", 20);

            var novelContent = new AtLeastSingleSpaceString(_novel.Content);

            var result = new NSMutableAttributedString(novelContent.PlainText);

            var paragraphStyle = CreateDefaultParagraph();
            result.AddAttribute(UIStringAttributeKey.ParagraphStyle, paragraphStyle, new NSRange(0, novelContent.Length));

            result.AddAttribute(UIStringAttributeKey.Font, UIFont.FromName("TimesNewRomanPSMT", 20), new NSRange(0, novelContent.Length));

            foreach (var textInfo in _novel.Format)
            {
                switch (textInfo.Style)
                {
                    case TextStyle.Centered:
                        var paragraph = CreateDefaultParagraph();
                        paragraph.Alignment = UITextAlignment.Center;
                        result.AddAttribute(UIStringAttributeKey.ParagraphStyle, paragraph, new NSRange(textInfo.Range.Offset, textInfo.Range.Length));
                        break;
                    case TextStyle.Right:
                        var paragraph2 = CreateDefaultParagraph();
                        paragraph2.Alignment = UITextAlignment.Right;
                        result.AddAttribute(UIStringAttributeKey.ParagraphStyle, paragraph2, new NSRange(textInfo.Range.Offset, textInfo.Range.Length));
                        break;
                    case TextStyle.Bold:
                        result.AddAttribute(UIStringAttributeKey.Font, UIFont.FromName("TimesNewRomanPS-BoldMT", 20), new NSRange(textInfo.Range.Offset, textInfo.Range.Length));
                        break;
                    case TextStyle.Italic:
                        result.AddAttribute(UIStringAttributeKey.Font, UIFont.FromName("TimesNewRomanPS-ItalicMT", 20), new NSRange(textInfo.Range.Offset, textInfo.Range.Length));
                        break;
                    case TextStyle.BoldAndItalic:
                        result.AddAttribute(UIStringAttributeKey.Font, UIFont.FromName("TimesNewRomanPS-BoldItalicMT", 20), new NSRange(textInfo.Range.Offset, textInfo.Range.Length));
                        break;
                }
            }

            MarkSpelling(result);

            _content.AttributedText = result;

            Add(_content);

            _cursor = new CursorView();
            InitCursor();

            InitSpellchecker();

            Layout = () =>
            {
                _content.ChangeSize(Frame.Width - _margin * 2, Frame.Height);
                _content.ChangeX(_margin);

                _cursor.PositionToCenterInside(this);
            };

            Layout();
        }

        private void ContentOnChanged(object sender, EventArgs e)
        {
            if (_shouldCheckSpelling)
            {
                MarkSpellingForCursorRange();
            }

            _save.AttemptToRun();
        }

        private NSMutableAttributedString MarkSpelling(NSMutableAttributedString result)
        {
            result.RemoveAttribute(UIStringAttributeKey.UnderlineStyle, new NSRange(0, result.Length));

            var spellChecker = new SpellChecker(_novel.Content.PlainText);

            foreach (var misspelledWord in spellChecker.MisspelledWords)
            {
                result.AddAttribute(UIStringAttributeKey.UnderlineStyle, NSNumber.FromInt32((int)NSUnderlineStyle.Single), misspelledWord);
            }

            return result;
        }

        private void MarkSpelling()
        {
            _content.AttributedText = MarkSpelling(new NSMutableAttributedString(_content.AttributedText));
        }

        private void MarkSpellingForCursorRange()
        {
            var range = new SpaceSeparatedRange(_content.Text, (int)CursorPosition - 50, 100);

            var result = new NSMutableAttributedString(_content.AttributedText);

            result.RemoveAttribute(UIStringAttributeKey.UnderlineStyle, new NSRange(range.StartIndex, range.Length));

            foreach (var misspelledWord in new MisspelledWordsSequence(new UITextChecker(), _content.Text, range))
            {
                result.AddAttribute(UIStringAttributeKey.UnderlineStyle, NSNumber.FromInt32((int)NSUnderlineStyle.Single), misspelledWord);
            }

            _content.AttributedText = result;
        }

        private void InitSpellchecker()
        {
            _timer = new Timer(OnSpellCheckTimer, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2));
        }

        private void OnSpellCheckTimer(object state)
        {
            
        }

        private NSMutableParagraphStyle CreateDefaultParagraph()
        {
            var paragraphStyle = new NSMutableParagraphStyle();
            paragraphStyle.FirstLineHeadIndent = 42;
            return paragraphStyle;
        }

        public override CGSize SizeThatFits(CGSize size)
        {
            return _content.SizeThatFits(new CGSize(size.Width - _margin*2, size.Height));
        }

        private void OnSelectionChanged(object sender, EventArgs e)
        {
            _cursorPosition = CursorPosition;
        }

        private nint CursorPosition
        {
            get => _content.GetOffsetFromPosition(_content.BeginningOfDocument, _content.SelectedTextRange.End);
            set
            {
                var cursorPosition = _content.GetPosition(_content.BeginningOfDocument, value);

                cursorPosition = cursorPosition ?? _content.GetPosition(_content.BeginningOfDocument, 0);

                _content.SelectedTextRange = _content.GetTextRange(cursorPosition, cursorPosition);
            }
        }

        private CGPoint CursorLocation
        {
            get
            {
                var cursorPosition = _content.GetPosition(_content.BeginningOfDocument, CursorPosition);

                var cursorFrame = _content.GetCaretRectForPosition(cursorPosition);

                return cursorFrame.Location;
            }
        }

        private void LeftCurrentLine()
        {
            ChangeCurrentParagraphStyle(x => x.Alignment = UITextAlignment.Left);
        }

        private void RightCurrentLine()
        {
            ChangeCurrentParagraphStyle(x => x.Alignment = UITextAlignment.Right);
        }

        private void CenterCurrentLine()
        {
            ChangeCurrentParagraphStyle(x => x.Alignment = UITextAlignment.Center);
        }

        private void ChangeStyle(Action changeStyle)
        {
            var cursorPosition = _content.GetPosition(_content.BeginningOfDocument, _cursorPosition);

            changeStyle();

            _changed = true;

            _content.SelectedTextRange = _content.GetTextRange(cursorPosition, cursorPosition);

            _content.ScrollRangeToVisible(_content.SelectedRange);
        }

        private void ChangeCurrentParagraphStyle(Action<NSMutableParagraphStyle> changeParagraphStyle)
        {
            ChangeStyle(() =>
            {
                var p = SelectedParagraph;

                var result = new NSMutableAttributedString(_content.AttributedText);

                result.RemoveAttribute(UIStringAttributeKey.ParagraphStyle, p);

                var paragraph = CreateDefaultParagraph();

                changeParagraphStyle(paragraph);

                result.AddAttribute(UIStringAttributeKey.ParagraphStyle, paragraph, p);

                _content.AttributedText = result;
            });
        }

        private NSRange SelectedParagraph
        {
            get
            {
                var selectedRange = _content.SelectedRange;

                var start = (int)selectedRange.Location;
                var end = (int)selectedRange.Location;

                var text = _content.Text;

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
                    for (int i = new Number(start, 0, text.Length - 1).Value; i > 0; i--)
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

        private bool _shouldCheckSpelling;

        private bool OnShouldChangeText(UITextView textview, NSRange range, string text)
        {
            _changed = true;

            _shouldCheckSpelling = text == " " || text.Length > 1;

            return true;
        }

        public void Hide()
        {
            Application.Instance.UnregisterScheme(_editModeScheme);
            Application.Instance.UnregisterScheme(_viewModeScheme);

            _wasFocused = Application.Instance.IsInEditMode;

            if (Focused)
            {
                _cursorPosition = CursorPosition;
            }

            _content.ResignFirstResponder();

            Application.Instance.ModeChanged -= InstanceOnModeChanged;
        }

        public void Show()
        {
            Application.Instance.RegisterSchemeForEditMode(_editModeScheme);
            Application.Instance.RegisterSchemeForViewMode(_viewModeScheme);

            if (_wasFocused)
            {
                _content.BecomeFirstResponder();

                var cursorPosition = _content.GetPosition(_content.BeginningOfDocument, _cursorPosition);

                _content.SelectedTextRange = _content.GetTextRange(cursorPosition, cursorPosition);
            }
            else
            {
                _content.ResignFirstResponder();
            }

            Application.Instance.ModeChanged += InstanceOnModeChanged;
        }

        public string Title => _novel.Title;

        public IPath Source => _novel.Source;

        public IText Content => new ThreadSafeProperty<IText>(() => new StringText(_content.Text)).Value;

        public ISequence<ITextInfo> Format => new ThreadSafeProperty<ISequence<ITextInfo>>(() => new ArraySequence<ITextInfo>(GetFormatting().ToArray())).Value;

        private IEnumerable<ITextInfo> GetFormatting()
        {
            var result = new List<ITextInfo>();

            var content = _content.Text;

            _content.AttributedText.EnumerateAttributes(new NSRange(0, content.Length), NSAttributedStringEnumeration.None, Callback);

            void Callback(NSDictionary attrs, NSRange range, ref bool stop)
            {
                var substring = new SubstringText(content, (int)range.Location, (int)range.Length);

                foreach (var attribute in attrs)
                {
                    var key = (NSMutableString)attribute.Key;

                    if (attribute.Value is NSMutableParagraphStyle p)
                    {
                        var style = GetStyle(p.Alignment);

                        if (style != TextStyle.None)
                        {
                            result.Add(new TextInfo(substring, style));
                        }
                    }

                    if (attribute.Value is UIFont f)
                    {
                        var style = GetStyleByFont(f);

                        if (style != TextStyle.None)
                        {
                            result.Add(new TextInfo(substring, style));
                        }
                    }
                }

                TextStyle GetStyleByFont(UIFont font)
                {
                    switch (font.Name)
                    {
                        case "TimesNewRomanPS-BoldMT": return TextStyle.Bold;
                        case "TimesNewRomanPS-ItalicMT": return TextStyle.Italic;
                        case "TimesNewRomanPS-BoldItalicMT": return TextStyle.BoldAndItalic;
                        default: return TextStyle.None;
                    }
                }

                TextStyle GetStyle(UITextAlignment textAlignment)
                {
                    switch (textAlignment)
                    {
                        case UITextAlignment.Center:
                            return TextStyle.Centered;
                        case UITextAlignment.Right:
                            return TextStyle.Right;
                        default:
                            return TextStyle.None;
                    }
                }
            }

            return result;
        }

        public void Pause()
        {
            Hide();
        }

        public void Resume()
        {
            Show();
        }
    }
}