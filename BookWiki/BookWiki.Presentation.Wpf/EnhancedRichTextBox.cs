using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using BookWiki.Core;
using BookWiki.Core.Utils;
using BookWiki.Presentation.Wpf.Models.SpellCheckModels;
using Keurig.IQ.Core.CrossCutting.Extensions;

namespace BookWiki.Presentation.Wpf
{
    public class EnhancedRichTextBox : RichTextBox
    {
        public EnhancedRichTextBox()
        {
            FontFamily = new FontFamily("Times New Roman");
            FontSize = 18;
            Language = XmlLanguage.GetLanguage("ru");
            BorderThickness = new Thickness(0, 0, 0, 0);

            PreviewKeyDown += OnPreviewKeyDown;
            PreviewMouseLeftButtonUp += OnPreviewMouseLeftButtonUp;
        }

        private void OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftAlt))
            {
                var cursorOffset = Document.ContentStart.GetOffsetToPosition(CaretPosition);

                var substrings = new PunctuationSeparatedEnumeration(Document, CaretPosition.Paragraph).ToArray();

                var selectedSubstring = substrings.FirstOrDefault(x => cursorOffset >= x.StartIndex && cursorOffset < x.EndIndex);

                if (selectedSubstring != null)
                {
                    BookShelf.Instance.OpenArticleOrSearch(selectedSubstring.Text);
                }
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyboardDevice.IsKeyDown(Key.LeftAlt))
            {
                if (e.KeyboardDevice.IsKeyDown(Key.Right))
                {
                    if (e.KeyboardDevice.IsKeyDown(Key.LeftShift))
                    {
                        Selection.Select(Selection.Start, GetThisLineEnd());

                        e.Handled = true;
                    }
                    else
                    {
                        CaretPosition = GetThisLineEnd();

                        e.Handled = true;
                    }
                }

                if (e.KeyboardDevice.IsKeyDown(Key.Left))
                {
                    if (e.KeyboardDevice.IsKeyDown(Key.LeftShift))
                    {
                        Selection.Select(CaretPosition.GetLineStartPosition(0), Selection.End);

                        e.Handled = true;
                    }
                    else
                    {
                        CaretPosition = CaretPosition.GetLineStartPosition(0);

                        e.Handled = true;
                    }
                }
            }

            if (e.Key == Key.D2 && e.KeyboardDevice.Modifiers == ModifierKeys.Shift)
            {
                var text = new TextRange(Document.ContentStart, CaretPosition).Text;

                var closings = new IndexSequenceV2(text, '»').SelfOrEmpty();
                var openings = new IndexSequenceV2(text, '«').SelfOrEmpty();

                var toInsert = closings.Count() < openings.Count() ? "»" : "«";

                Insert(toInsert);

                e.Handled = true;
            }

            if (e.Key == Key.OemMinus)
            {
                var previousPosition = CaretPosition.GetNextInsertionPosition(LogicalDirection.Backward);

                var isItNotALetterOrNumber = false;

                if (previousPosition == null)
                {
                    isItNotALetterOrNumber = true;
                }
                else if (TextParts.NotALetterOrNumber.Contains(new TextRange(previousPosition, CaretPosition).Text[0]))
                {
                    isItNotALetterOrNumber = true;
                }

                if (isItNotALetterOrNumber)
                {
                    Insert("–");

                    e.Handled = true;
                }
            }

            base.OnKeyDown(e);
        }

        public void Prettify()
        {
            char? previous = null;
            var cursor = Document.ContentStart;

            while (true)
            {
                var nextToCursor = cursor.GetNextInsertionPosition(LogicalDirection.Forward);

                if (nextToCursor == null)
                {
                    return;
                }

                var currentText = new TextRange(cursor, nextToCursor).Text;

                if (currentText.Length > 0)
                {
                    char current = currentText[0];

                    if (current == '-')
                    {
                        var isItNotALetterOrNumber = false;

                        if (previous == null)
                        {
                            isItNotALetterOrNumber = true;
                        }
                        else if (TextParts.NotALetterOrNumber.Contains(previous.Value))
                        {
                            isItNotALetterOrNumber = true;
                        }

                        if (isItNotALetterOrNumber)
                        {
                            Selection.Select(cursor, nextToCursor);
                            Selection.Text = "–";
                        }
                    }

                    previous = current;
                }
                
                cursor = nextToCursor;
            }
        }

        TextPointer GetThisLineEnd()
        {
            var isEof = CaretPosition.GetLineStartPosition(1) == null;

            if (isEof)
            {
                return Document.ContentEnd;
            }

            var currentLineOffset = Document.ContentStart.GetOffsetToPosition(CaretPosition.GetLineStartPosition(0));
            var nextLineOffset = Document.ContentStart.GetOffsetToPosition(CaretPosition.GetLineStartPosition(1));

            for (int i = nextLineOffset; i > currentLineOffset; i--)
            {
                var possibleEol = Document.ContentStart.GetPositionAtOffset(i);

                var offsetToCheck = Document.ContentStart.GetOffsetToPosition(possibleEol.GetLineStartPosition(0));

                if (currentLineOffset == offsetToCheck)
                {
                    return possibleEol;
                }
            }

            return null;
        }

        private void Insert(string toInsert)
        {
            var currentCaretPosition = Document.ContentStart.GetOffsetToPosition(CaretPosition);

            Selection.Start.InsertTextInRun(toInsert);

            CaretPosition = Document.ContentStart.GetPositionAtOffset(currentCaretPosition + 1);
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (BookShelf.Instance.KeyProcessor.Handle(e.KeyboardDevice))
            {
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Back)
            {
                if (CaretPosition.IsAtLineStartPosition)
                {
                    if (string.IsNullOrWhiteSpace(Selection.Text))
                    {
                        var previousEnd = CaretPosition.GetLineStartPosition(-1);

                        if (previousEnd != null)
                        {
                            var previousLine = GetLineEnd(previousEnd);

                            Selection.Select(previousLine.GetInsertionPosition(LogicalDirection.Backward), CaretPosition);

                            Selection.Text = "";

                            CaretPosition = Selection.End;

                            e.Handled = true;
                        }
                    }
                }
            }
        }

        TextPointer GetLineEnd(TextPointer position)
        {
            var isEof = position.GetLineStartPosition(1) == null;

            if (isEof)
            {
                return Document.ContentEnd;
            }

            var currentLineOffset = Document.ContentStart.GetOffsetToPosition(position.GetLineStartPosition(0));
            var nextLineOffset = Document.ContentStart.GetOffsetToPosition(position.GetLineStartPosition(1));

            for (int i = nextLineOffset; i > currentLineOffset; i--)
            {
                var possibleEol = Document.ContentStart.GetPositionAtOffset(i);

                var offsetToCheck = Document.ContentStart.GetOffsetToPosition(possibleEol.GetLineStartPosition(0));

                if (currentLineOffset == offsetToCheck)
                {
                    return possibleEol;
                }
            }

            return null;
        }
    }
}