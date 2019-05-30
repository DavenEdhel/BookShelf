using System;
using System.Timers;

namespace BookWiki.Core.Utils
{
    public class DeferredAction
    {
        private readonly Action _action;
        private readonly Timer _checker;

        public DeferredAction(TimeSpan delayTime, Action action)
        {
            _action = action;
            _checker = new Timer(delayTime.TotalMilliseconds);
            _checker.Elapsed += CheckerOnElapsed;
        }

        private void CheckerOnElapsed(object sender, ElapsedEventArgs e)
        {
            _checker.Stop();

            _action();
        }

        public void AttemptToRun()
        {
            _checker.Stop();
            _checker.Start();
        }
    }
}