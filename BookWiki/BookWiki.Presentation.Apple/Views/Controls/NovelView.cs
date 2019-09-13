using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    public class NovelView : View, IContentView, INovel
    {
        private readonly INovel _novel;
        private readonly ILibrary _library;

        private bool _wasFocused;
        private HotKeyScheme _viewModeScheme;
        private HotKeyScheme _editModeScheme;
        private EditTextView _content;
        private int _margin = 24;

        private Logger _logger = new Logger("NovelView");

        public NovelView(INovel novel, ILibrary library)
        {
            _novel = novel;
            _library = library;

            var scrollUp = new HotKey(Key.ArrowUp, () => _content.ScrollUp());
            var scrollDown = new HotKey(Key.ArrowDown, () => _content.ScrollDown());
            var save = new HotKey(new KeyCombination(new Key("s"), UIKeyModifierFlags.Control), () => Save());

            _viewModeScheme = new HotKeyScheme(scrollUp, scrollDown, save);
            _editModeScheme = new HotKeyScheme(save);

            Initialize();
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

        private PageNumberView _pageNumber;

        public void Hide()
        {
            Application.Instance.UnregisterScheme(_editModeScheme);
            Application.Instance.UnregisterScheme(_viewModeScheme);

            _wasFocused = Application.Instance.IsInEditMode;

            _content.Pause();

            _content.DeactivateEditMode();

            Application.Instance.ModeChanged -= InstanceOnModeChanged;

            Save();
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
    }
}