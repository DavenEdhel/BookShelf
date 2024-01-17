using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using BookWiki.Core;
using BookWiki.Presentation.Wpf.Models.SpellCheckModels;

namespace BookWiki.Presentation.Wpf.Models
{
    public class ParagraphFromBlock : IParagraph
    {
        public ParagraphFromBlock(Paragraph p)
        {
            switch (p.TextAlignment)
            {
                case TextAlignment.Left:
                    FormattingStyle = TextStyle.None;
                    break;
                case TextAlignment.Right:
                    FormattingStyle = TextStyle.Right;
                    break;
                case TextAlignment.Center:
                    FormattingStyle = TextStyle.Centered;
                    break;
            }

            var result = new List<IInline>();

            foreach (var pInline in p.Inlines.Enumerate())
            {
                result.Add(new InlineFromRichTextBoxInline(pInline));
            }

            Inlines = result.ToArray();
        }

        public TextStyle FormattingStyle { get; }
        public IInline[] Inlines { get; }
    }
}