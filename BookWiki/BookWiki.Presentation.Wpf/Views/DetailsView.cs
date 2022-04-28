using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using BookWiki.Core;
using BookWiki.Presentation.Wpf.Models;

namespace BookWiki.Presentation.Wpf.Views
{
    public class DetailsView : ScrollViewer
    {
        private readonly EnhancedRichTextBox _rtb;

        public DetailsView()
        {
            Content = _rtb = new EnhancedRichTextBox();

            Height = 450;
            _rtb.Background = new SolidColorBrush(Colors.White);
        }

        public IText AllData => new FormattedContentFromDocumentFlow(new DocumentFlowContentFromRichTextBox(_rtb)).Content;

        public void LoadFrom(IText novelComments)
        {
            new DocumentFlowContentFromTextAndFormat(novelComments, new EmptySequence<ITextInfo>()).ReloadInto(_rtb);
        }
    }
}