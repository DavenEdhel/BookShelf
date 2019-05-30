using System;
using BookWiki.Core;
using BookWiki.Presentation.Apple.Controllers;
using BookWiki.Presentation.Apple.Models;
using BookWiki.Presentation.Apple.Models.HotKeys;
using Foundation;
using UIKit;

namespace BookWiki.Presentation.Apple.Views.Controls
{
    public class ArticleView : UITextView, IView, IContentView
    {
        private readonly IArticle _article;

        private bool _wasFocused;
        private nint _cursorPosition;
        private HotKeyScheme _viewModeScheme;
        private HotKeyScheme _editModeScheme;

        public ArticleView(IArticle article)
        {
            _article = article;
            _article.SetViewport(this);

            var deactivateEditMode = new HotKey(Key.Escape, DeactivateEditMode);
            var activateEditMode = new HotKey(Key.Enter, ActivateEditMode);

            _viewModeScheme = new HotKeyScheme(activateEditMode);
            _editModeScheme = new HotKeyScheme(deactivateEditMode);

            Initialize();
        }

        private void ActivateEditMode()
        {
            BecomeFirstResponder();
        }

        private void DeactivateEditMode()
        {
            ResignFirstResponder();
        }

        private void Initialize()
        {
            Editable = true;
            AllowsEditingTextAttributes = true;
            ShouldChangeText += OnShouldChangeText;
            AutocorrectionType = UITextAutocorrectionType.No;
            AutocapitalizationType = UITextAutocapitalizationType.None;
            SelectionChanged += OnSelectionChanged;

            Render();
        }

        private void OnSelectionChanged(object sender, EventArgs e)
        {
            _cursorPosition = GetOffsetFromPosition(BeginningOfDocument, SelectedTextRange.End);
        }

        public void Render()
        {
            AttributedText = GetAttributedStringFromArticle();
        }

        private bool OnShouldChangeText(UITextView textview, NSRange range, string text)
        {
            try
            {
                if (range.Length > 0)
                {
                    _article.RemovePart((int)range.Location, (int)range.Length);
                }
                else
                {
                    _article.InsertPart((int)range.Location, text);
                }

                return true;
            }
            catch (Exception e)
            {
                if (range.Length > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"RemovePart({(int)range.Location}, {(int)range.Length})");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"InsertPart({(int)range.Location}, {text})");
                }

                System.Diagnostics.Debug.WriteLine(e.ToString());

                throw;
            }
        }

        private NSAttributedString GetAttributedStringFromArticle()
        {
            var content = _article.Content.PlainText;

            var result = new NSMutableAttributedString(content);

            var offset = 0;

            foreach (var articlePart in _article.ToArticleParts())
            {
                switch (articlePart.MarkerStyle)
                {
                    case MarkerStyle.Text:
                        result.AddAttribute(UIStringAttributeKey.Font, UIFont.SystemFontOfSize(17), new NSRange(offset, articlePart.Text.Length));
                        break;
                    case MarkerStyle.Header:
                        result.AddAttribute(UIStringAttributeKey.Font, UIFont.BoldSystemFontOfSize(20), new NSRange(offset, articlePart.Text.Length));
                        break;
                    case MarkerStyle.Link:
                        result.AddAttribute(UIStringAttributeKey.Font, UIFont.ItalicSystemFontOfSize(17), new NSRange(offset, articlePart.Text.Length));
                        break;
                }

                offset += articlePart.Text.Length;
            }

            var paragraphStyle = new NSMutableParagraphStyle();
            paragraphStyle.ParagraphSpacingBefore = 20;
            paragraphStyle.ParagraphSpacing = 10;
            paragraphStyle.FirstLineHeadIndent = 20;
            result.AddAttribute(UIStringAttributeKey.ParagraphStyle, paragraphStyle, new NSRange(0, offset));

            return result;
        }

        public void Hide()
        {
            Application.Instance.UnregisterScheme(_editModeScheme);
            Application.Instance.UnregisterScheme(_viewModeScheme);

            _wasFocused = Application.Instance.IsInEditMode;

            if (Focused)
            {
                _cursorPosition = GetOffsetFromPosition(BeginningOfDocument, SelectedTextRange.End);
            }

            ResignFirstResponder();
        }

        public void Show()
        {
            Application.Instance.RegisterSchemeForEditMode(_editModeScheme);
            Application.Instance.RegisterSchemeForViewMode(_viewModeScheme);

            if (_wasFocused)
            {
                BecomeFirstResponder();

                var cursorPosition = GetPosition(BeginningOfDocument, _cursorPosition);

                SelectedTextRange = GetTextRange(cursorPosition, cursorPosition);
            }
            else
            {
                ResignFirstResponder();
            }
        }
    }
}