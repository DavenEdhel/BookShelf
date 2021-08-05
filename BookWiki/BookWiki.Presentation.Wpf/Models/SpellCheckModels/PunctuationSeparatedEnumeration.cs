using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;
using BookWiki.Core.Utils.TextModels;

namespace BookWiki.Presentation.Wpf.Models.SpellCheckModels
{
    public class PunctuationSeparatedEnumeration : IEnumerable<ISubstring>
    {
        private readonly FlowDocument _document;
        private readonly Paragraph _p;

        public PunctuationSeparatedEnumeration(FlowDocument document, Paragraph p)
        {
            _document = document;
            _p = p;
        }

        public IEnumerator<ISubstring> GetEnumerator()
        {
            var i = new PunctuationSeparatedEnumerationV2(_p.Inlines.Select(x => new OffsetText()
            {
                Offset = _document.ContentStart.GetOffsetToPosition(x.ContentStart),
                Text = FlowDocumentExtensions.GetText(x)
            })).ToArray();

            return i.ToList().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}