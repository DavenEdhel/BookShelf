using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

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