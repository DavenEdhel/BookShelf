using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using BookWiki.Core;
using BookWiki.Core.Fb2Models;
using BookWiki.Core.Files.FileModels;
using BookWiki.Core.Files.PathModels;
using BookWiki.Core.FileSystem.FileModels;
using BookWiki.Core.LifeSpellCheckModels;
using BookWiki.Core.Logging;
using BookWiki.Core.Utils.TextModels;
using BookWiki.Presentation.Wpf.Models;
using BookWiki.Presentation.Wpf.Models.QuickNavigationModels;
using BookWiki.Presentation.Wpf.Models.SpellCheckModels;
using BookWiki.Presentation.Wpf.Views;
using Keurig.IQ.Core.CrossCutting.Extensions;
using TextCopy;

namespace BookWiki.Presentation.Wpf
{
    /// <summary>
    /// Interaction logic for ArticleWindow.xaml
    /// </summary>
    public partial class ArticleWindow : Window, IHighlightCollection
    {
        private readonly Article _article;
        private readonly IRelativePath _novel;
        private readonly Logger _logger = new Logger(nameof(NovelWindow));
        private bool _isChanged = false;
        private bool _canBeClosed = false;
        private bool _requestToClose = false;
        private CancellationTokenSource _token;
        private LifeSearchEngine _lifeSearchEngine;
        private OpenedTabsView _openedTabs;
        private RightSideViewV2 _rightSide;

        public bool ClosingFailed { get; set; } = false;

        public IRelativePath Novel => _novel;

        public ArticleWindow(Article article)
        {
            _article = article;
            _metadata = new ArticleMetadata(article.Source.AbsolutePath(BookShelf.Instance.RootPath));
            BookShelf.Instance.PageConfig.Changed += PageConfigOnChanged;

            _novel = article.Source;
            InitializeComponent();

            _openedTabs = new OpenedTabsView();
            _openedTabs.Width = 300;
            _openedTabs.HorizontalAlignment = HorizontalAlignment.Left;
            _openedTabs.Visibility = Visibility.Visible;
            _openedTabs.Margin = new Thickness(0);
            _openedTabs.MouseDown += ChangeOpenedTabsVisibility;
            Grid.SetColumn(_openedTabs, 0);
            Root.Children.Add(_openedTabs);

            _rightSide = new RightSideViewV2();
            _rightSide.HorizontalAlignment = HorizontalAlignment.Right;
            _rightSide.Visibility = Visibility.Hidden;
            _rightSide.Margin = new Thickness(0);
            _rightSide.MouseDown += ChangeRightSideVisibility;
            Grid.SetColumn(_rightSide, 2);
            Root.Children.Add(_rightSide);

            Title = new NovelTitle(article.Source).PlainText;
            
            _lifeSearchEngine = new LifeSearchEngine(Rtb, this, SearchBox, Scroll);

            _specialItemsHighlighter = new NavigateToArticleEngine(Rtb, this, Scroll);

            LoadContent(article);

            PageConfigOnChanged(BookShelf.Instance.PageConfig.Current);

            _token = new CancellationTokenSource();
            RunAutosave();

            _openedTabs.Start();
            _rightSide.Start();

            ApplyHeightAdjustments();
        }


        private void ApplyHeightAdjustments()
        {
            this.MinHeight -= BookShelf.Instance.Config.HeightModification;
            Height -= BookShelf.Instance.Config.HeightModification;
            NovelContentGrid.Height -= BookShelf.Instance.Config.HeightModification;
            _rightSide.Margin = new Thickness(0, 0, 0, BookShelf.Instance.Config.HeightModification);
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
            else
            {
                _openedTabs.Stop();
                _rightSide.Stop();
            }

            base.OnClosing(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            BookShelf.Instance.ReportWindowClosed();
        }

        private void PageConfigOnChanged(UserInterfaceSettings obj)
        {
            if (IsScrollVisible() != !obj.IsScrollHidden)
            {
                ToggleScroll();
            }
        }

        private void MarkChanged(object sender, TextChangedEventArgs e)
        {
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

                    _article.Refresh();

                    _metadata.Save(new ArticleMetadata.Data()
                    {
                        Name = ArticleName.Text,
                        NameVariations = NameVariations.Text.Split(' '),
                        Tags = Tags.Text.Split(' ')
                    });

                    // todo: images
                }

                return true;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString(), $"Something went wrong with {Novel.Name.PlainText}");

                return false;
            }
        }

        private void LoadContent(Article article)
        {
            new DocumentFlowContentFromTextAndFormat(article).ReloadInto(Rtb);

            _isChanged = false;
            SaveButton.Visibility = Visibility.Hidden;

            ArticleName.Text = article.Name;
            NameVariations.Text = article.NameVariations.JoinStringsWithoutSkipping(" ");
            Tags.Text = article.Tags.JoinStringsWithoutSkipping(" ");
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

        private void SelectionChanged(object sender, RoutedEventArgs e)
        {
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
                    if (new SpellCheckV2(BookShelf.Instance.Dictionary, BookShelf.Instance.Articles).IsCorrect(selectedSubstring.Text) == false)
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

        private void SaveContent(object sender, RoutedEventArgs e)
        {
            Save();

            SaveButton.Visibility = Visibility.Hidden;
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

            if (e.Key == Key.Escape)
            {
                Close();
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

        public void Highlight(ISubstring toHighlight, bool specialStyle, Action<string> onClick = null)
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
                    Tag = toHighlight.Text
                };

                r.MouseUp += (sender, args) =>
                {
                    onClick?.Invoke(sender.CastTo<Rectangle>().Tag.ToString());
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

        private void TopBarMouseDown(object sender, MouseButtonEventArgs e)
        {
            TopBarGrid.Visibility = (TopBarGrid.Visibility == Visibility.Visible) ? Visibility.Hidden : Visibility.Visible;
        }

        private int _usualHeight = 890;
        private int _usualWidth = 734;
        private readonly NavigateToArticleEngine _specialItemsHighlighter;
        private readonly ArticleMetadata _metadata;

        private void OnResize(object sender, SizeChangedEventArgs e)
        {
            _openedTabs.ToggleVisibility(CanTabsBeVisible(e.NewSize.Width));
            _rightSide.ToggleVisibility(CanTabsBeVisible(e.NewSize.Width));
        }

        private bool CanTabsBeVisible(double width) => width > _usualWidth + 200 * 2 + 5;

        private void ChangeOpenedTabsVisibility(object sender, MouseButtonEventArgs e)
        {
            if (CanTabsBeVisible(ActualWidth))
            {
                _openedTabs.ToggleVisibility();
            }

        }

        private void ChangeRightSideVisibility(object sender, MouseButtonEventArgs e)
        {
            if (CanTabsBeVisible(ActualWidth))
            {
                _rightSide.ToggleVisibility();
            }
        }

        private void TextSelectedAndClicked(object sender, MouseButtonEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                var cursorOffset = Rtb.Document.ContentStart.GetOffsetToPosition(Rtb.CaretPosition);

                var substrings = new PunctuationSeparatedEnumeration(Rtb.Document, Rtb.CaretPosition.Paragraph).ToArray();

                var selectedSubstring = substrings.FirstOrDefault(x => cursorOffset >= x.StartIndex && cursorOffset < x.EndIndex);

                if (selectedSubstring != null)
                {
                    if (_specialItemsHighlighter.IsApplicable(selectedSubstring))
                    {
                        _specialItemsHighlighter.Navigate(selectedSubstring);
                        return;
                    }

                    new WordInfoWindow(selectedSubstring.Text).ShowDialog();
                }
            }
        }
    }
}
