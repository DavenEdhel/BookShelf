using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using BookWiki.Core;

namespace BookWiki.Presentation.Wpf.Views
{
    public static class DocumentFlowContentExtensions
    {
        public static void LoadInto(this IDocumentFlowContent rtf, RichTextBox view)
        {
            view.Document.Blocks.Clear();

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

                view.Document.Blocks.Add(paragraph);
            }
        }
    }
}