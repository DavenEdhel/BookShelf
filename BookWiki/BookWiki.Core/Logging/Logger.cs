namespace BookWiki.Core.Logging
{
    public class Logger
    {
        private readonly string _prefix;

        public Logger(string prefix)
        {
            _prefix = prefix;
        }

        public void Info(string info)
        {
            System.Diagnostics.Debug.WriteLine($"{_prefix}: {info}");
        }
    }
}