using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using BookWiki.Core;
using BookWiki.Core.Files.FileModels;
using BookWiki.Core.Files.PathModels;
using BookWiki.Core.FileSystem.FileModels;
using BookWiki.Core.LifeSpellCheckModels;
using BookWiki.Core.Logging;
using BookWiki.Core.Utils;
using BookWiki.Core.Utils.TextModels;
using BookWiki.Presentation.Wpf.Models;
using BookWiki.Presentation.Wpf.Models.QuickNavigationModels;
using BookWiki.Presentation.Wpf.Models.SpellCheckModels;
using Keurig.IQ.Core.CrossCutting.Extensions;

namespace BookWiki.Presentation.Wpf
{
    /// <summary>
    /// Interaction logic for NovelWindow.xaml
    /// </summary>
    public partial class NovelWindow : Window, IErrorsCollectionV2, IHighlightCollection
    {
        private readonly IRelativePath _novel;
        private readonly LifeSpellCheckV2 _lifeSpellCheck;
        private readonly Logger _logger = new Logger(nameof(NovelWindow));
        private bool _isChanged = false;
        private bool _canBeClosed = false;
        private bool _requestToClose = false;
        private CancellationTokenSource _token;
        private LifeSearchEngine _lifeSearchEngine;

        public bool ClosingFailed { get; set; } = false;

        public IRelativePath Novel => _novel;

        public NovelWindow(Novel novel)
        {
            BookShelf.Instance.PageConfig.Changed += PageConfigOnChanged;

            _novel = novel.Source;
            InitializeComponent();

            Title = new NovelTitle(novel.Source).PlainText;

            Rtb.FontFamily = new FontFamily("Times New Roman");
            Rtb.FontSize = 18;
            Rtb.Language = XmlLanguage.GetLanguage("ru");
            Rtb.BorderThickness = new Thickness(0, 0, 0, 0);

            _lifeSpellCheck = new LifeSpellCheckV2(Rtb, this, new SpellCheckV2(BookShelf.Instance.Dictionary));
            _lifeSearchEngine = new LifeSearchEngine(Rtb, this, SearchBox, Scroll);

            LoadContent(novel);

            PageConfigOnChanged(BookShelf.Instance.PageConfig.Current);

            _token = new CancellationTokenSource();
            RunAutosave();
        }

        public NovelWindow(IRelativePath novel) : this(new Novel(novel, BookShelf.Instance.RootPath))
        {
            
        }

        private async Task RunAutosave()
        {
            while (true)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(30), _token.Token);
                }
                catch (Exception e)
                {
                }

                if (_requestToClose)
                {
                    if (Save())
                    {
                        BookShelf.Instance.PageConfig.Changed -= PageConfigOnChanged;

                        _canBeClosed = true;

                        Dispatcher.InvokeAsync(Close);

                        ClosingFailed = false;

                        return;
                    }
                    else
                    {
                        _token = new CancellationTokenSource();

                        _requestToClose = false;

                        ClosingFailed = true;
                    }
                }
                else
                {
                    if (Save())
                    {
                        SaveButton.Visibility = Visibility.Hidden;
                    }
                }
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            _requestToClose = true;

            _token.Cancel();

            if (_canBeClosed == false)
            {
                e.Cancel = true;
            }

            base.OnClosing(e);
        }

        private void PageConfigOnChanged(UserInterfaceSettings obj)
        {
            _pageMode = PageNumber.PageModes[obj.PageModeIndex];
            UpdatePaging();

            if (IsSpellcheckOn() != obj.IsSpellCheckOn)
            {
                ToggleSpellcheck();
            }

            if (IsScrollVisible() != !obj.IsScrollHidden)
            {
                ToggleScroll();
            }
        }

        private void Content_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            _lifeSpellCheck.TextChangedInside(Rtb.CaretPosition.Paragraph);

            UpdatePaging();

            _isChanged = true;

            SaveButton.Visibility = Visibility.Visible;
        }

        public bool Save()
        {
            try
            {
                lock (this)
                {
                    var c = new DocumentFlowContentFromRichTextBox(Rtb);

                    var formattedContent = new FormattedContentFromDocumentFlow(c);

                    var file = new ContentFolder(_novel.AbsolutePath(BookShelf.Instance.RootPath));
                    file.Save(formattedContent);
                }

                return true;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString(), $"Something went wrong with {Novel.Name.PlainText}");

                return false;
            }
        }

        private void LoadContent(Novel novel)
        {
            var rtf = new DocumentFlowContentFromTextAndFormat(novel);

            Rtb.Document.Blocks.Clear();

            foreach (var rtfParagraph in rtf.Paragraphs)
            {
                var paragraph = new Paragraph();

                switch (rtfParagraph.FormattingStyle)
                {
                    case TextStyle.Centered:
                        paragraph.TextAlignment = TextAlignment.Center;
                        break;
                    case TextStyle.Right:
                        paragraph.TextAlignment = TextAlignment.Right;
                        break;
                }

                foreach (var rtfParagraphInline in rtfParagraph.Inlines)
                {
                    var inline = new Run(rtfParagraphInline.Text.PlainText);

                    switch (rtfParagraphInline.TextStyle)
                    {
                        case TextStyle.Bold:
                            paragraph.Inlines.Add(new Bold(inline));
                            break;
                        case TextStyle.Italic:
                            paragraph.Inlines.Add(new Italic(inline));
                            break;
                        case TextStyle.BoldAndItalic:
                            paragraph.Inlines.Add(new Italic(new Bold(inline)));
                            break;
                        default:
                            paragraph.Inlines.Add(inline);
                            break;
                    }
                }

                Rtb.Document.Blocks.Add(paragraph);
            }

            _isChanged = false;
            SaveButton.Visibility = Visibility.Hidden;
        }

        private void ToRight(object sender, RoutedEventArgs e)
        {
            var p = Rtb.CaretPosition.Paragraph;

            if (p != null)
            {
                p.TextAlignment = TextAlignment.Right;
            }
        }

        private void ToLeft(object sender, RoutedEventArgs e)
        {
            var p = Rtb.CaretPosition.Paragraph;

            if (p != null)
            {
                p.TextAlignment = TextAlignment.Left;
            }
        }

        private void ToCenter(object sender, RoutedEventArgs e)
        {
            var p = Rtb.CaretPosition.Paragraph;

            if (p != null)
            {
                p.TextAlignment = TextAlignment.Center;
            }
        }

        public void Add(ISubstring error)
        {
            var x1 = Rtb.Document.ContentStart.GetPositionAtOffset(error.StartIndex).GetCharacterRect(LogicalDirection.Forward);
            var x2 = Rtb.Document.ContentStart.GetPositionAtOffset(error.EndIndex).GetCharacterRect(LogicalDirection.Forward);

            if (x2.X > x1.X)
            {
                var r = new Rectangle()
                {
                    Width = x2.X - x1.X,
                    Height = 1,
                    Stroke = Brushes.LightCoral,
                    Stretch = Stretch.Fill
                };

                Canvas.SetTop(r, x1.Bottom - 3);
                Canvas.SetLeft(r, x1.X);

                SpellCheckBox.Children.Add(r);
            }
        }

        public void RemoveAll()
        {
            SpellCheckBox.Children.Clear();
        }

        private void SelectionChanged(object sender, RoutedEventArgs e)
        {
            _lifeSpellCheck.CursorPositionChanged();
        }

        private string _pageMode = PageNumber.Pages;

        private int Length => new TextRange(Rtb.Document.ContentStart, Rtb.Document.ContentEnd).Text.Length;

        private void UpdatePagingForAuthorLists()
        {
            var listSize = 40000;

            var als = (double)Length / (double)listSize;

            Pages.Text = $"{als:##.000} а.л.";
        }

        private void UpdatePagingForChars()
        {
            var ks = Length / 1000f;

            Pages.Text = $"{ks:###.0}k";
        }

        private void UpdatePagingForPages()
        {
            var pageSize = 1020;

            var totalPages = (int)(Rtb.ActualHeight / pageSize) + 1;

            var currentPage = (int)(Scroll.ContentVerticalOffset / pageSize) + 1;

            Pages.Text = $"{currentPage} из {totalPages}";
        }

        private void UpdatePaging()
        {
            if (_pageMode == PageNumber.Pages)
            {
                UpdatePagingForPages();
            }

            if (_pageMode == PageNumber.NotDiplay)
            {
                UpdatePagingForNotDisplay();
            }

            if (_pageMode == PageNumber.AuthorLists)
            {
                UpdatePagingForAuthorLists();
            }

            if (_pageMode == PageNumber.Characters)
            {
                UpdatePagingForChars();
            }
        }

        private void UpdatePagingForNotDisplay()
        {
            Pages.Text = "None";
        }

        private void ChangeMode(object sender, MouseButtonEventArgs e)
        {
            _pageMode = PageNumber.PageModes[(PageNumber.PageModes.IndexOf(x => x == _pageMode) + 1) % PageNumber.PageModes.Length];

            UpdatePaging();

            BookShelf.Instance.PageConfig.SetDisplayMode(_pageMode);
        }

        private void Scroll_OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (_pageMode == PageNumber.Pages)
            {
                UpdatePagingForPages();
            }
        }

        private void LearnNewWordFromCursor(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Q && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                var cursorOffset = Rtb.Document.ContentStart.GetOffsetToPosition(Rtb.CaretPosition);

                var substrings = new PunctuationSeparatedEnumeration(Rtb.Document, Rtb.CaretPosition.Paragraph).ToArray();

                var selectedSubstring = substrings.FirstOrDefault(x => cursorOffset >= x.StartIndex && cursorOffset < x.EndIndex);

                if (selectedSubstring != null)
                {
                    if (new SpellCheckV2(BookShelf.Instance.Dictionary).IsCorrect(selectedSubstring.Text) == false)
                    {
                        BookShelf.Instance.Dictionary.Learn(selectedSubstring.Text);
                    }
                }
            }
        }

        private void Rtb_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.W && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                BookShelf.Instance.ShowFileSystem();

                e.Handled = true;
            }

            if (e.Key == Key.S && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                Save();

                SaveButton.Visibility = Visibility.Hidden;

                e.Handled = true;
            }

            if (e.KeyboardDevice.IsKeyDown(Key.LeftAlt))
            {
                if (e.KeyboardDevice.IsKeyDown(Key.Right))
                {
                    if (e.KeyboardDevice.IsKeyDown(Key.LeftShift))
                    {
                        Rtb.Selection.Select(Rtb.Selection.Start, GetThisLineEnd());

                        e.Handled = true;
                    }
                    else
                    {
                        Rtb.CaretPosition = GetThisLineEnd();

                        e.Handled = true;
                    }
                }

                if (e.KeyboardDevice.IsKeyDown(Key.Left))
                {
                    if (e.KeyboardDevice.IsKeyDown(Key.LeftShift))
                    {
                        Rtb.Selection.Select(Rtb.CaretPosition.GetLineStartPosition(0), Rtb.Selection.End);

                        e.Handled = true;
                    }
                    else
                    {
                        Rtb.CaretPosition = Rtb.CaretPosition.GetLineStartPosition(0);

                        e.Handled = true;
                    }
                }
            }

            if (e.Key == Key.D2 && e.KeyboardDevice.Modifiers == ModifierKeys.Shift)
            {
                var text = new TextRange(Rtb.Document.ContentStart, Rtb.CaretPosition).Text;

                var closings = new IndexSequenceV2(text, '»').SelfOrEmpty();
                var openings = new IndexSequenceV2(text, '«').SelfOrEmpty();

                var toInsert = closings.Count() < openings.Count() ? "»" : "«";

                Insert(toInsert);

                e.Handled = true;
            }
        }

        TextPointer GetLineEnd(TextPointer position)
        {
            var isEof = position.GetLineStartPosition(1) == null;

            if (isEof)
            {
                return Rtb.Document.ContentEnd;
            }

            var currentLineOffset = Rtb.Document.ContentStart.GetOffsetToPosition(position.GetLineStartPosition(0));
            var nextLineOffset = Rtb.Document.ContentStart.GetOffsetToPosition(position.GetLineStartPosition(1));

            for (int i = nextLineOffset; i > currentLineOffset; i--)
            {
                var possibleEol = Rtb.Document.ContentStart.GetPositionAtOffset(i);

                var offsetToCheck = Rtb.Document.ContentStart.GetOffsetToPosition(possibleEol.GetLineStartPosition(0));

                if (currentLineOffset == offsetToCheck)
                {
                    return possibleEol;
                }
            }

            return null;
        }

        TextPointer GetThisLineEnd()
        {
            var isEof = Rtb.CaretPosition.GetLineStartPosition(1) == null;

            if (isEof)
            {
                return Rtb.Document.ContentEnd;
            }

            var currentLineOffset = Rtb.Document.ContentStart.GetOffsetToPosition(Rtb.CaretPosition.GetLineStartPosition(0));
            var nextLineOffset = Rtb.Document.ContentStart.GetOffsetToPosition(Rtb.CaretPosition.GetLineStartPosition(1));

            for (int i = nextLineOffset; i > currentLineOffset; i--)
            {
                var possibleEol = Rtb.Document.ContentStart.GetPositionAtOffset(i);

                var offsetToCheck = Rtb.Document.ContentStart.GetOffsetToPosition(possibleEol.GetLineStartPosition(0));

                if (currentLineOffset == offsetToCheck)
                {
                    return possibleEol;
                }
            }

            return null;
        }

        private void Insert(string toInsert)
        {
            var currentCaretPosition = Rtb.Document.ContentStart.GetOffsetToPosition(Rtb.CaretPosition);

            Rtb.Selection.Start.InsertTextInRun(toInsert);

            Rtb.CaretPosition = Rtb.Document.ContentStart.GetPositionAtOffset(currentCaretPosition + 1);
        }

        private void ScrollSwitch(object sender, RoutedEventArgs e)
        {
            ToggleScroll();

            BookShelf.Instance.PageConfig.SetScrollVisibility(ScrollSwitchButton.Content.ToString() == "Scroll Visible");
        }

        private void ToggleScroll()
        {
            if (IsScrollVisible())
            {
                ScrollSwitchButton.Content = "Scroll Hidden";

                Scroll.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;

                Rtb.Margin = new Thickness(0, 0, 17, 0);
            }
            else
            {
                ScrollSwitchButton.Content = "Scroll Visible";

                Scroll.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;

                Rtb.Margin = new Thickness(0, 0, 0, 0);
            }
        }

        private bool IsScrollVisible()
        {
            return ScrollSwitchButton.Content.ToString() == "Scroll Visible";
        }

        private void SpellcheckSwitch(object sender, RoutedEventArgs e)
        {
            ToggleSpellcheck();

            BookShelf.Instance.PageConfig.SetSpellcheckAvailability(SpellcheckSwitchButton.Content.ToString() == "Spellcheck On");
        }

        private void ToggleSpellcheck()
        {
            if (IsSpellcheckOn())
            {
                SpellcheckSwitchButton.Content = "Spellcheck Off";

                _lifeSpellCheck.IsEnabled = false;
            }
            else
            {
                SpellcheckSwitchButton.Content = "Spellcheck On";

                _lifeSpellCheck.IsEnabled = true;

                _lifeSpellCheck.CursorPositionChanged();
            }
        }

        private bool IsSpellcheckOn()
        {
            return SpellcheckSwitchButton.Content.ToString() == "Spellcheck On";
        }

        private void SaveContent(object sender, RoutedEventArgs e)
        {
            Save();

            SaveButton.Visibility = Visibility.Hidden;
        }

        private void Rtb_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (BookShelf.Instance.KeyProcessor.Handle(e.KeyboardDevice))
            {
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Back)
            {
                if (Rtb.CaretPosition.IsAtLineStartPosition)
                {
                    if (string.IsNullOrWhiteSpace(Rtb.Selection.Text))
                    {
                        var previousEnd = Rtb.CaretPosition.GetLineStartPosition(-1);

                        if (previousEnd != null)
                        {
                            var previousLine = GetLineEnd(previousEnd);

                            Rtb.Selection.Select(previousLine.GetInsertionPosition(LogicalDirection.Backward), Rtb.CaretPosition);

                            Rtb.Selection.Text = "";

                            Rtb.CaretPosition = Rtb.Selection.End;

                            e.Handled = true;
                        }
                    }
                }
            }
        }

        private void NovelWindow_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (BookShelf.Instance.KeyProcessor.Handle(e.KeyboardDevice))
            {
                e.Handled = true;
                return;
            }

            if (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) && e.KeyboardDevice.IsKeyDown(Key.F))
            {
                SearchBox.Focus();
                return;
            }
        }

        private void OnSearchPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (BookShelf.Instance.KeyProcessor.Handle(e.KeyboardDevice))
            {
                e.Handled = true;
                return;
            }
        }

        private void OnSearchKeyDown(object sender, KeyEventArgs e)
        {
            
        }

        public void Highlight(ISubstring toHighlight, bool specialStyle)
        {
            var x1 = Rtb.Document.ContentStart.GetPositionAtOffset(toHighlight.StartIndex).GetCharacterRect(LogicalDirection.Forward);
            var x2 = Rtb.Document.ContentStart.GetPositionAtOffset(toHighlight.EndIndex).GetCharacterRect(LogicalDirection.Forward);

            if (x2.X > x1.X)
            {
                var r = new Rectangle()
                {
                    Width = x2.X - x1.X,
                    Height = Math.Abs(x1.Top - x1.Bottom),
                    Stroke = specialStyle ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.LightBlue),
                    Stretch = Stretch.Fill,
                    Fill = new SolidColorBrush(Color.FromArgb(60, Colors.LightBlue.R, Colors.LightBlue.G, Colors.LightBlue.B)),
                };

                Canvas.SetTop(r, x1.Top);
                Canvas.SetLeft(r, x1.X);

                HighlightBox.Children.Add(r);
            }
        }

        public void ScrollTo(ISubstring toScroll)
        {
            var x1 = Rtb.Document.ContentStart.GetPositionAtOffset(toScroll.StartIndex).GetCharacterRect(LogicalDirection.Forward);
            var x2 = Rtb.Document.ContentStart.GetPositionAtOffset(toScroll.EndIndex).GetCharacterRect(LogicalDirection.Forward);

            if (x1.Top > Scroll.VerticalOffset && x2.Top < (Scroll.VerticalOffset + Scroll.ActualHeight - 50))
            {
                return;
            }

            Scroll.ScrollToVerticalOffset(x1.Top - Scroll.ActualHeight / 2);
        }

        public void ClearHighlighting()
        {
            HighlightBox.Children.Clear();
        }
    }
}
