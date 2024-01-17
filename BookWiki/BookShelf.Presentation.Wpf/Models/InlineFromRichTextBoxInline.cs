using System;
using System.Windows.Documents;
using BookWiki.Core;
using Keurig.IQ.Core.CrossCutting.Extensions;
using Inline = System.Windows.Documents.Inline;

namespace BookWiki.Presentation.Wpf.Models
{
    public class InlineFromRichTextBoxInline : IInline
    {
        public InlineFromRichTextBoxInline(Inline inline)
        {
            switch (inline)
            {
                case Bold bold:
                    if (bold.Inlines.FirstInline is Italic boldItalic)
                    {
                        TextStyle = TextStyle.BoldAndItalic;
                        Text = new StringText(boldItalic.Inlines.FirstInline.CastTo<Run>().Text);
                    }
                    else
                    {
                        TextStyle = TextStyle.Bold;
                        Text = new StringText(bold.Inlines.FirstInline.CastTo<Run>().Text);
                    }
                    break;
                case Italic italic:
                    if (italic.Inlines.FirstInline is Bold italicBolc)
                    {
                        TextStyle = TextStyle.BoldAndItalic;
                        Text = new StringText(italicBolc.Inlines.FirstInline.CastTo<Run>().Text);
                    }
                    else
                    {
                        TextStyle = TextStyle.Italic;
                        Text = new StringText(italic.Inlines.FirstInline.CastTo<Run>().Text);
                    }
                    break;
                case Run run:
                    TextStyle = TextStyle.None;
                    Text = new StringText(run.Text);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(inline));
            }
        }

        public TextStyle TextStyle { get; }
        public IText Text { get; set; }
    }
}