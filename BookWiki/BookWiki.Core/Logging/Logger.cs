using System.Collections.Generic;

namespace BookWiki.Core.Logging
{
    public class Logger
    {
        public static List<ILogSource> Sources { get; } = new List<ILogSource>();

        private static readonly object _globalLock = new object();

        private readonly string _prefix;

        public Logger(string prefix)
        {
            _prefix = prefix;
        }

        public void Info(string info)
        {
            var entry = new LogEntry(info);

            lock (_globalLock)
            {
                Sources.ForEach(x => x.Log(entry));
            }
            
            System.Diagnostics.Debug.WriteLine($"{_prefix}: {info}");
        }
    }

    public interface ILogSource
    {
        void Log(ILogEntry entry);
    }

    public interface ILogEntry
    {
        string Message { get; }
    }

    public class LogEntry : ILogEntry
    {
        public LogEntry(string message)
        {
            Message = message;
        }

        public string Message { get; }
    }
}