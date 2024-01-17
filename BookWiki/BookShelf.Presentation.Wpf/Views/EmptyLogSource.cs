using BookWiki.Core.Logging;

namespace BookWiki.Presentation.Wpf.Views
{
    public class EmptyLogSource : ILogSource
    {
        public void Log(ILogEntry entry)
        {
        }
    }
}