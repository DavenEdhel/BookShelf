using System.Windows.Documents;
using System.Windows.Threading;
using BookWiki.Core.Logging;

namespace BookWiki.Presentation.Wpf.Views
{
    public class ConsoleLogSource : ILogSource
    {
        private readonly EnhancedRichTextBox _output;
        private readonly Dispatcher _dispatcher;

        public ConsoleLogSource(EnhancedRichTextBox output, Dispatcher dispatcher)
        {
            _output = output;
            _dispatcher = dispatcher;
        }

        public void Log(ILogEntry entry)
        {
            _dispatcher.Invoke(() =>
            {
                _output.Document.Blocks.Add(new Paragraph(new Run(entry.Message)));
            });
        }
    }
}