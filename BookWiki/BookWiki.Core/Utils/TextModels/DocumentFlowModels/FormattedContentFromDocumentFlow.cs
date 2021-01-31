using System.Collections.Generic;
using System.Text;

namespace BookWiki.Core
{
    public class FormattedContentFromDocumentFlow : IFormattedContent
    {
        public FormattedContentFromDocumentFlow(IDocumentFlowContent documentFlow)
        {
            var text = new StringBuilder();
            var infos = new List<ITextInfo>();

            var offset = 0;

            foreach (var p in documentFlow.Paragraphs)
            {
                var pStart = offset;

                foreach (var i in p.Inlines)
                {
                    var iStart = offset;

                    text.Append(i.Text.PlainText);

                    offset += i.Text.Length;

                    if (i.TextStyle != TextStyle.None)
                    {
                        infos.Add(new TextInfo(new MutableRange()
                        {
                            Length = offset - iStart,
                            Offset = iStart
                        }, i.TextStyle));
                    }
                }

                text.Append("\n");
                offset++;

                if (p.FormattingStyle != TextStyle.None && offset - pStart > 1)
                {
                    infos.Add(new TextInfo(new MutableRange()
                    {
                        Length = offset - pStart,
                        Offset = pStart
                    }, p.FormattingStyle));
                }
            }

            Content = new StringText(text.ToString());
            Format = new ArraySequence<ITextInfo>(infos.ToArray());
        }

        public ISequence<ITextInfo> Format { get; }
        public IText Content { get; }
    }
}