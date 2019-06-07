using System;
using System.Collections.Generic;
using System.Linq;
using BookWiki.Core;
using BookWiki.Core.Files.PathModels;
using BookWiki.Core.Logging;
using BookWiki.Core.Utils;
using BookWiki.Presentation.Apple.Controllers;
using BookWiki.Presentation.Apple.Extentions;
using BookWiki.Presentation.Apple.Models;
using BookWiki.Presentation.Apple.Models.HotKeys;
using BookWiki.Presentation.Apple.Models.Utils;
using BookWiki.Presentation.Apple.Views.Common;
using CoreGraphics;
using Foundation;
using UIKit;

namespace BookWiki.Presentation.Apple.Views.Controls
{
    public class NovelView : View, IContentView, INovel, IKeyboardListener
    {
        private readonly INovel _novel;
        private readonly ILibrary _library;

        private bool _wasFocused;
        private HotKeyScheme _viewModeScheme;
        private HotKeyScheme _editModeScheme;
        private EditTextView _content;
        private int _margin = 24;

        private bool _changed = false;
        private IPath _source;

        private DeferredAction _save;

        private Logger _logger = new Logger("NovelView");

        public NovelView(INovel novel, ILibrary library)
        {
            _novel = novel;
            _library = library;

            var deactivateEditMode = new HotKey(Key.Escape, () => _content.DeactivateEditMode());
            var activateEditMode = new HotKey(Key.Enter, () => _content.ActivateEditMode());
            var leftTheLine = new HotKey(new Key("7"), LeftCurrentLine).WithCommand();
            var centerTheLine = new HotKey(new Key("8"), CenterCurrentLine).WithCommand();
            var rightTheLine = new HotKey(new Key("9"), RightCurrentLine).WithCommand();
            var enableJoButton = new HotKey(new Key("]"), EnableJoButton);
            var enableJoButtonUpperCase = new HotKey(new Key("]"), EnableJoButtonUpperCase).WithShift();
            var showAutocorrection = new HotKey(new KeyCombination(Key.Space, UIKeyModifierFlags.Shift), ShowAutocorrection);
            var moveCursorToRight = new HotKey(new Key("j"), ScrollUp);
            var moveCursorToLeft = new HotKey(new Key("k"), ScrollDown);

            _viewModeScheme = new HotKeyScheme(activateEditMode, moveCursorToLeft, moveCursorToRight);
            _editModeScheme = new HotKeyScheme(deactivateEditMode, leftTheLine, centerTheLine, rightTheLine, enableJoButton, enableJoButtonUpperCase, showAutocorrection);

            _save = new DeferredAction(TimeSpan.FromSeconds(10), () => _library.Update(this));

            Initialize();
        }

        private void ScrollDown()
        {
            var delta = _content.Frame.Height / 2;

            _content.SetContentOffset(new CGPoint(0, _content.ContentOffset.Y + delta), true);
        }

        private void ScrollUp()
        {
            var delta = _content.Frame.Height / 2;

            _content.SetContentOffset(new CGPoint(0, _content.ContentOffset.Y - delta), true);
        }

        private void ShowAutocorrection()
        {
            var spellChecker = new SpellChecker(_content.Text, (int)_content.CursorPosition);

            if (spellChecker.IsCursorInMisspelledWord)
            {
                var box = new AutocorectionBoxView(spellChecker, ReplaceOnCurrentCursorWith, NewWordLearned);

                box.Show(this);
            }
        }

        private void NewWordLearned()
        {
            _content.CheckSpelling();
        }

        private void ReplaceOnCurrentCursorWith(NSRange misspelledWord, string word)
        {
            _content.ReplaceMisspelledWord(misspelledWord, word);
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

        private void Initialize()
        {
            _content = new EditTextView();
            _content.DefaultParagraph = () => new NSMutableParagraphStyle()
            {
                FirstLineHeadIndent = 42
            };
            _content.Editable = true;
            _content.AllowsEditingTextAttributes = true;
            _content.ShouldChangeText += OnShouldChangeText;
            _content.Changed += ContentOnChanged;
            _content.AutocorrectionType = UITextAutocorrectionType.No;
            _content.AutocapitalizationType = UITextAutocapitalizationType.None;
            _content.ShowsVerticalScrollIndicator = false;
            _content.Font = UIFont.FromName("TimesNewRomanPSMT", 20);
            _content.ContentInset = new UIEdgeInsets(0, 0, 50, 0);
            _content.Scrolled += ContentOnScrolled;

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

            result = MarkSpelling(result);

            _content.AttributedText = result;

            Add(_content);

            _pageNumber = new UILabel();
            _pageNumber.Font = UIFont.BoldSystemFontOfSize(20);
            _pageNumber.TextAlignment = UITextAlignment.Right;
            _pageNumber.TextColor = UIColor.LightGray;
            UpdatePaging();
            Add(_pageNumber);

            Layout = () =>
            {
                _content.ChangeSize(Frame.Width - _margin * 2, Frame.Height);
                _content.ChangeX(_margin);

                _pageNumber.SetSizeThatFits();
                _pageNumber.ChangeWidth(200);
                _pageNumber.PositionToRightAndBottomInside(this, 5, 5);
            };

            Layout();
        }

        private void ContentOnScrolled(object sender, EventArgs e)
        {
            UpdatePaging();
        }

        private void ContentOnChanged(object sender, EventArgs e)
        {
            if (_shouldCheckSpelling)
            {
                _content.MarkSpellingForCursorRange();
            }

            _save.AttemptToRun();

            UpdatePaging();
        }

        private void UpdatePaging()
        {
            var pageSize = 1120;

            var totalPages = (int)(_content.ContentSize.Height / pageSize) + 1;

            var currentPage = (int) (_content.ContentOffset.Y / pageSize) + 1;

            _pageNumber.Text = $"{currentPage} из {totalPages}";
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

        private void LeftCurrentLine()
        {
            _content.ChangeCurrentParagraphStyle(x => x.Alignment = UITextAlignment.Left);
        }

        private void RightCurrentLine()
        {
            _content.ChangeCurrentParagraphStyle(x => x.Alignment = UITextAlignment.Right);
        }

        private void CenterCurrentLine()
        {
            _content.ChangeCurrentParagraphStyle(x => x.Alignment = UITextAlignment.Center);
        }

        private bool _shouldCheckSpelling;
        private UILabel _pageNumber;

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

            _content.DeactivateEditMode();

            Application.Instance.ModeChanged -= InstanceOnModeChanged;
        }

        public void Show()
        {
            Application.Instance.RegisterSchemeForEditMode(_editModeScheme);
            Application.Instance.RegisterSchemeForViewMode(_viewModeScheme);

            if (_wasFocused)
            {
                _content.ActivateEditMode();
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