using System.Collections.Generic;
using System.Linq;

namespace BookWiki.Core
{
    public class DocumentFlowContentFromTextAndFormat : IDocumentFlowContent
    {
        public DocumentFlowContentFromTextAndFormat(IFormattedContent formattedContent, bool removeCaretReturn) : this(formattedContent.Content, formattedContent.Format, removeCaretReturn)
        {


        }

        public DocumentFlowContentFromTextAndFormat(IText text, ISequence<ITextInfo> format, bool removeCaretReturn)
        {
            if (removeCaretReturn)
            {
                text = new StringText(text.PlainText.Replace("\r", ""));
            }

            var indexes = new IndexSequenceV2(text.PlainText, '\n').ToArray();

            var paragraphs = new List<IParagraph>();

            if (indexes.Any())
            {
                var i = 0;

                foreach (var index in indexes)
                {
                    paragraphs.Add(new ParagraphFromTextRange(text, text.Substring(i, index - i), format));

                    i = index;
                }

                if (i < text.Length)
                {
                    paragraphs.Add(new ParagraphFromTextRange(text, text.Substring(i, text.Length - i), format));
                }

                Paragraphs = paragraphs.ToArray();
            }
            else
            {
                Paragraphs = new IParagraph[]
                {
                    new ParagraphFromTextRange(text, text.Substring(0, text.Length), format)
                };
            }   
        }

        public IParagraph[] Paragraphs { get; }
    }
}