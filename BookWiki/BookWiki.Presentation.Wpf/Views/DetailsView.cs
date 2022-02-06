using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using BookWiki.Core;
using BookWiki.Presentation.Wpf.Models;

namespace BookWiki.Presentation.Wpf.Views
{
    public class DetailsView : EnhancedRichTextBox
    {
        public DetailsView()
        {
            Height = 450;
            Background = new SolidColorBrush(Colors.White);
            FontSize = 15;
            Margin = new Thickness(30, 0, 30, 0);
        }

        public IText AllData => new FormattedContentFromDocumentFlow(new DocumentFlowContentFromRichTextBox(this)).Content;

        public void LoadFrom(IText novelComments)
        {
            new DocumentFlowContentFromTextAndFormat(novelComments, new EmptySequence<ITextInfo>()).LoadInto(this);
        }
    }
}