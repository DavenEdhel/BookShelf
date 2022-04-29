using BookWiki.Core.Logging;

namespace BookWiki.Presentation.Wpf.Views
{
    public class LogSourceHolder
    {
        private ILogSource _logSource;
        private bool _isStarted;

        public ILogSource LogSource
        {
            get => _logSource;
            set
            {
                if (_isStarted)
                {
                    Logger.Sources.Remove(_logSource);
                }

                _logSource = value;

                if (_isStarted)
                {
                    Logger.Sources.Add(_logSource);
                }
            }
        }

        public void Start()
        {
            _isStarted = true;

            Logger.Sources.Add(_logSource);
        }

        public void Stop()
        {
            _isStarted = false;

            Logger.Sources.Remove(_logSource);
        }
    }
}