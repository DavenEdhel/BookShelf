using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Documents;
using BookWiki.Core;
using BookWiki.Core.Files.PathModels;

namespace BookWiki.Presentation.Wpf.Models
{
    public class DocumentFlowContentFromRichTextBox : IDocumentFlowContent
    {
        public DocumentFlowContentFromRichTextBox(RichTextBox rtb)
        {
            var result = new List<IParagraph>();

            foreach (var block in rtb.Document.Blocks)
            {
                if (block is Paragraph p)
                {
                    result.Add(new ParagraphFromBlock(p));
                }
            }

            Paragraphs = result.ToArray();
        }

        public IParagraph[] Paragraphs { get; }
    }
}