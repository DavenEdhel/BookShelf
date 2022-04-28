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
using BookWiki.Core.Files.FileSystemModels;
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

namespace BookWiki.Presentation.Wpf
{
    /// <summary>
    /// Interaction logic for NovelWindow.xaml
    /// </summary>
    public partial class CompiledNovelWindow : Window
    {
        private readonly IRelativePath _novel;
        private readonly Logger _logger = new Logger(nameof(NovelWindow));
        private bool _requestToClose = false;
        private OpenedTabsView _openedTabs;
        private RightSideViewV2 _rightSide;

        public bool ClosingFailed { get; set; } = false;

        public CompiledNovelWindow(IFileSystemNode compiledBook)
        {
            BookShelf.Instance.PageConfig.Changed += PageConfigOnChanged;

            var statistics = new NodeFolder(compiledBook.Path);

            InitializeComponent();

            _openedTabs = new OpenedTabsView();
            _openedTabs.Width = 300;
            _openedTabs.HorizontalAlignment = HorizontalAlignment.Left;
            _openedTabs.Visibility = Visibility.Hidden;
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

            Title = new CompilationTitle(compiledBook).Value;

            LoadContent(statistics.Load());

            Pages.Novel = Rtb;
            Pages.Scroll = Scroll;
            Pages.Start();

            PageConfigOnChanged(BookShelf.Instance.PageConfig.Current);

            _openedTabs.Start();
            _rightSide.Start();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            _requestToClose = true;

            _openedTabs.Stop();
            _rightSide.Stop();
            Pages.Stop();

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

        private void LoadContent(IRelativePath[] novelPaths)
        {
            foreach (var relativePath in novelPaths)
            {
                var novel = new Novel(relativePath, BookShelf.Instance.RootPath);

                new DocumentFlowContentFromTextAndFormat(novel).LoadInto(Rtb);
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

        private void TopBarMouseDown(object sender, MouseButtonEventArgs e)
        {
            TopBarGrid.Visibility = (TopBarGrid.Visibility == Visibility.Visible) ? Visibility.Hidden : Visibility.Visible;
        }

        private int _usualHeight = 890;
        private int _usualWidth = 734;

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
    }
}
