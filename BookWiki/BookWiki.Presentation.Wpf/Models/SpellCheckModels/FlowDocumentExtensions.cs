using System;
using System.Windows.Documents;

namespace BookWiki.Presentation.Wpf.Models.SpellCheckModels
{
    public static class FlowDocumentExtensions
    {
        public static string GetText(this Inline inline)
        {
            switch (inline)
            {
                case Bold bold:
                    return GetText(bold.Inlines.FirstInline);
                case Italic italic:
                    return GetText(italic.Inlines.FirstInline);
                case Run run:
                    return run.Text;
                default:
                    throw new ArgumentOutOfRangeException(nameof(inline));
            }
        }
    }
}