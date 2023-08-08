using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using BookWiki.Core;
using BookWiki.Core.Utils;
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
                var previousChar = new TextRange(CaretPosition.GetNextInsertionPosition(LogicalDirection.Backward), CaretPosition);

                if (TextParts.NotALetterOrNumber.Contains(previousChar.Text[0]))
                {
                    Insert("–");

                    e.Handled = true;
                }
            }

            base.OnKeyDown(e);
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