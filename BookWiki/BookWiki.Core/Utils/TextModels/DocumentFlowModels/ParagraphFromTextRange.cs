using System.Collections.Generic;
using System.Linq;
using BookWiki.Core.Search;
using BookWiki.Core.Utils.TextModels;
using BookWiki.Presentation.Apple.Models.Utils;

namespace BookWiki.Core
{
    public class ParagraphFromTextRange : IParagraph
    {
        public ParagraphFromTextRange(IText origin, ITextRange substring, ISequence<ITextInfo> textInfos)
        {
            var stylesForParagraph = textInfos.Where(x => x.Range.InV2(substring).PartiallyOrCompletely());

            FormattingStyle = stylesForParagraph.FirstOrDefault(x => x.Style == TextStyle.Centered || x.Style == TextStyle.Right)?.Style ?? TextStyle.None;

            var offset = 0;

            if (substring.PlainText.Contains('\n'))
            {
                offset = 1;
            }

            if (substring.PlainText.Contains('\r'))
            {
                offset = 2;
            }

            var paragraphRange = new SimpleRange(substring.Length() - offset, substring.Offset);

            var textStyles = textInfos.Where(x => x.Style == TextStyle.Bold || x.Style == TextStyle.BoldAndItalic || x.Style == TextStyle.Italic).ToArray();

            var ranges = new TextIntervalEnumeration(origin, paragraphRange , textStyles.Select(x => x.Range).ToArray());

            if (ranges.Any())
            {
                var inlines = new List<IInline>();

                foreach (var range in ranges)
                {
                    var style = textStyles.FirstOrDefault(x => range.InV2(x.Range).PartiallyOrCompletely())?.Style ?? TextStyle.None;

                    inlines.Add(new Inline(range.GetPlainText(origin), style));
                }

                Inlines = inlines.ToArray();
            }
            else
            {
                Inlines = new IInline[]
                {
                    new Inline(origin.Substring(paragraphRange.Offset, paragraphRange.Length), TextStyle.None), 
                };
            }
        }
        
        public TextStyle FormattingStyle { get; }

        public IInline[] Inlines { get; }
    }
}