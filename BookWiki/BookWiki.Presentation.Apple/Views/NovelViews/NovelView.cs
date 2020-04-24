using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookWiki.Core;
using BookWiki.Core.Files.PathModels;
using BookWiki.Core.Logging;
using BookWiki.Core.Utils;
using BookWiki.Core.ViewModels;
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
    public class NovelView : View, IContentView, INovel
    {
        private readonly INovel _novel;
        private readonly ILibrary _library;
        private readonly ISaveStatus _saveStatus;
        private readonly ActionBarView _actionBarView;

        private bool _wasFocused;
        private HotKeyScheme _viewModeScheme;
        private HotKeyScheme _editModeScheme;
        private HotKeyScheme _localSearchScheme;
        private EditTextView _content;
        private int _contentWidth = 776;

        public bool WasFocused => _wasFocused;

        private Logger _logger = new Logger("NovelView");

        public NovelView(INovel novel, ILibrary library, ISaveStatus saveStatus, ActionBarView actionBarView)
        {
            _novel = novel;
            _library = library;
            _saveStatus = saveStatus;
            _actionBarView = actionBarView;

            var scrollUp = new HotKey(Key.ArrowUp, () => _content.ScrollUp());
            var scrollDown = new HotKey(Key.ArrowDown, () => _content.ScrollDown());
            var save = new HotKey(new KeyCombination(new Key("s"), UIKeyModifierFlags.Control), () => Save());
            var nextRightItem = new HotKey(new KeyCombination(Key.ArrowRight, UIKeyModifierFlags.Control), NextRightItem);
            var prevRightItem = new HotKey(new KeyCombination(Key.ArrowLeft, UIKeyModifierFlags.Control), PrevRightItem);
            var nextCenterItem = new HotKey(new KeyCombination(Key.ArrowDown, UIKeyModifierFlags.Control), NextCenterItem);
            var prevCenterItem = new HotKey(new KeyCombination(Key.ArrowUp, UIKeyModifierFlags.Control), PrevCenterItem);
            var findNextSelected = new HotKey(new KeyCombination(Key.Enter, UIKeyModifierFlags.Control), FindNextSelected);

            _viewModeScheme = new HotKeyScheme(scrollUp, scrollDown, save);
            _editModeScheme = new HotKeyScheme(save, nextRightItem, prevRightItem, nextCenterItem, prevCenterItem, findNextSelected);
            _localSearchScheme = new HotKeyScheme(
                new HotKey(Key.ArrowUp, () => _currentSearch.SelectPrev()),
                new HotKey(Key.ArrowDown, () => _currentSearch.SelectNext()),
                new HotKey(Key.Enter, () => _currentSearch.SelectNext()),
                new HotKey(Key.Escape, () =>
                {
                    _currentSearch.Remove();
                    _currentSearch = null;

                    Application.Instance.UnregisterScheme(_localSearchScheme);

                    Application.Instance.RegisterSchemeForEditMode(_editModeScheme);
                    Application.Instance.RegisterSchemeForViewMode(_viewModeScheme);

                    _content.Resume();
                }));

            Initialize();
        }

        private void FindNextSelected()
        {
            var selectedText = _content.SelectedText;

            if (string.IsNullOrWhiteSpace(selectedText))
            {
                return;
            }

            var nextFinding = _content.Text.IndexOf(selectedText, (int)_content.CursorPosition, StringComparison.InvariantCultureIgnoreCase);

            if (nextFinding <= 0)
            {
                nextFinding = _content.Text.IndexOf(selectedText, 0, StringComparison.InvariantCultureIgnoreCase);
            }

            _content.CursorPosition = nextFinding;
            _content.SelectTextRange(nextFinding, selectedText.Length);
        }

        private void PrevCenterItem()
        {
            var formatting = GetFormatting().ToArray();

            var nextItem = formatting
                               .Where(x => x.Range.Length > 1)
                               .Where(x => x.Style == TextStyle.Centered)
                               .OrderByDescending(x => x.Range.Offset)
                               .FirstOrDefault(x => x.Range.End() < _content.CursorPosition)
                           ?? formatting
                               .Where(x => x.Range.Length > 1)
                               .Where(x => x.Style == TextStyle.Centered)
                               .OrderByDescending(x => x.Range.Offset)
                               .FirstOrDefault();

            if (nextItem != null)
            {
                System.Diagnostics.Debug.WriteLine("Item found " + nextItem.Range.Offset);

                _content.CursorPosition = nextItem.Range.Middle();
            }
        }

        private void NextCenterItem()
        {
            var formatting = GetFormatting().ToArray();

            var nextItem = formatting
                               .Where(x => x.Range.Length > 1)
                               .Where(x => x.Style == TextStyle.Centered)
                               .OrderBy(x => x.Range.Offset)
                               .FirstOrDefault(x => x.Range.Start() > _content.CursorPosition)
                           ?? formatting
                               .Where(x => x.Range.Length > 1)
                               .Where(x => x.Style == TextStyle.Centered)
                               .OrderBy(x => x.Range.Offset)
                               .FirstOrDefault();

            if (nextItem != null)
            {
                System.Diagnostics.Debug.WriteLine("Item found " + nextItem.Range.Offset);

                _content.CursorPosition = nextItem.Range.Middle();
            }
        }

        private void PrevRightItem()
        {
            var formatting = GetFormatting().ToArray();

            var nextItem = formatting
                               .Where(x => x.Range.Length > 1)
                               .Where(x => x.Style == TextStyle.Right)
                               .OrderByDescending(x => x.Range.Offset)
                               .FirstOrDefault(x => x.Range.End() < _content.CursorPosition)
                           ?? formatting
                               .Where(x => x.Range.Length > 1)
                               .Where(x => x.Style == TextStyle.Right)
                               .OrderByDescending(x => x.Range.Offset)
                               .FirstOrDefault();

            if (nextItem != null)
            {
                System.Diagnostics.Debug.WriteLine("Item found " + nextItem.Range.Offset);

                _content.CursorPosition = nextItem.Range.Middle();
            }
        }

        private void NextRightItem()
        {
            var formatting = GetFormatting().ToArray();

            var nextItem = formatting
                               .Where(x => x.Range.Length > 1)
                               .Where(x => x.Style == TextStyle.Right)
                               .OrderBy(x => x.Range.Offset)
                               .FirstOrDefault(x => x.Range.Start() > _content.CursorPosition)
                           ?? formatting
                               .Where(x => x.Range.Length > 1)
                               .Where(x => x.Style == TextStyle.Right)
                               .OrderBy(x => x.Range.Offset)
                               .FirstOrDefault();

            if (nextItem != null)
            {
                System.Diagnostics.Debug.WriteLine("Item found " + nextItem.Range.Offset);

                _content.CursorPosition = nextItem.Range.Middle();
            }
        }

        private void InstanceOnModeChanged(bool obj)
        {

        }

        private void Initialize()
        {
            _content = new EditTextView();
            _content.Changed += ContentOnChanged;

            _content.DefaultParagraph = () => new NSMutableParagraphStyle()
            {
                FirstLineHeadIndent = 42
            };

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

            _content.AttributedText = result;

            Add(_content);

            _pageNumber = new PageNumberView();
            Add(_pageNumber);

            _pageNumber.BindWith(_content);

            _scrollBar = new ScrollBarView(_content);
            Add(_scrollBar);

            Layout = () =>
            {
                if (_isScrollHidden)
                {
                    var margin = (Frame.Width - _contentWidth) / 2;
                    var contentMargin = margin;

                    _content.ChangeSize(_contentWidth, Frame.Height);
                    _content.ChangeX(contentMargin);

                    _pageNumber.SetSizeThatFits();
                    _pageNumber.ChangeWidth(200);
                    _pageNumber.PositionToRightAndBottomInside(this, 5, 5);

                    _scrollBar.Hidden = true;
                }
                else
                {
                    var margin = (Frame.Width - _contentWidth) / 2;
                    var contentMargin = (nfloat) 0.25 * margin;

                    _content.ChangeSize(_contentWidth, Frame.Height);
                    _content.ChangeX(contentMargin);

                    _pageNumber.SetSizeThatFits();
                    _pageNumber.ChangeWidth(200);
                    _pageNumber.PositionToRightAndBottomInside(this, 5, 5);

                    _scrollBar.ChangeX(_content.Frame.Right + contentMargin);
                    _scrollBar.ChangeY(Frame.Top);
                    _scrollBar.ChangeSize(Frame.Width - (_content.Frame.Right + contentMargin), Frame.Height - _pageNumber.Frame.Height - 5);
                    _scrollBar.Hidden = false;
                }
            };

            Layout();

            _saveStatus.IsUpToDate = true;
        }

        private void ContentOnChanged(object sender, EventArgs e)
        {
            _saveStatus.IsUpToDate = false;
        }

        private NSMutableParagraphStyle CreateDefaultParagraph()
        {
            var paragraphStyle = new NSMutableParagraphStyle();
            paragraphStyle.FirstLineHeadIndent = 42;
            return paragraphStyle;
        }

        public override CGSize SizeThatFits(CGSize size)
        {
            var margin = (Frame.Width - _contentWidth) / 2;

            return _content.SizeThatFits(new CGSize(size.Width - margin*2, size.Height));
        }

        private PageNumberView _pageNumber;
        private bool _isActive;
        private ScrollBarView _scrollBar;
        private bool _isScrollHidden;
        public bool IsActive => _isActive;

        public void Hide()
        {
            Application.Instance.UnregisterScheme(_editModeScheme);
            Application.Instance.UnregisterScheme(_viewModeScheme);
            Application.Instance.UnregisterScheme(_localSearchScheme);

            _currentSearch?.Remove();
            _currentSearch = null;

            _wasFocused = Application.Instance.IsInEditMode;

            _content.Pause();

            _content.DeactivateEditMode();

            Application.Instance.ModeChanged -= InstanceOnModeChanged;

            Save();

            _isActive = false;
        }

        private void Save()
        {
            _library.Update(this);

            _library.Save();
        }

        public void Show()
        {
            Application.Instance.RegisterSchemeForEditMode(_editModeScheme);
            Application.Instance.RegisterSchemeForViewMode(_viewModeScheme);

            _content.Resume();

            if (_wasFocused)
            {
                _content.ActivateEditMode();
            }
            else
            {
                _content.ResignFirstResponder();
            }

            SetScrollVisibility(_actionBarView.ScrollState.IsOff);
            SetPageMode(_actionBarView.PageMode.Current);

            _isActive = true;

            Application.Instance.ModeChanged += InstanceOnModeChanged;
        }

        public string Title => _novel.Title;

        public IRelativePath Source => _novel.Source;

        public IText Content => new ThreadSafeProperty<IText>(() => new StringText(_content.Text)).Value;

        public ISequence<ITextInfo> Format => new ThreadSafeProperty<ISequence<ITextInfo>>(() => new ArraySequence<ITextInfo>(GetFormatting().ToArray())).Value;

        public IEditorState State
        {
            get => new NovelViewEditorState(this, _content);
            set
            {
                _content.CursorPosition = value.LastCaretPosition;
                _content.SetContentOffset(new CGPoint(0, value.ScrollPosition), false);

                if (value.IsEditing)
                {
                    _content.ActivateEditMode();
                }
                else
                {
                    _content.ResignFirstResponder();
                }
            }
        }

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

        public void SetScrollVisibility(bool b)
        {
            _isScrollHidden = b;

            Layout();
        }

        private LocalSearchItemCollection _currentSearch;

        public void BeginSearchEnumeration(IQuery searchQuery)
        {
            if (_currentSearch == null)
            {
                HightlightQuery(searchQuery);
            }

            _currentSearch.SelectNext();

            _content.Pause();

            Application.Instance.RegisterScheme(_localSearchScheme);
        }

        public void HightlightQuery(IQuery searchQuery)
        {
            _currentSearch?.Remove();

            _currentSearch = new LocalSearchItemCollection(searchQuery, this, _content);
            _currentSearch.Apply();
        }

        public void SetPageMode(string pageModeCurrent)
        {
            _pageNumber.SetPageMode(pageModeCurrent);
        }
    }
}