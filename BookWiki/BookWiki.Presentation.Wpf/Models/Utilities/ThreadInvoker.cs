namespace BookWiki.Presentation.Wpf.Models.Utilities
{
    using System;
    using System.Threading.Tasks;

    public class ThreadInvoker : IThreadInvoker
    {
        public void InvokeInBackgroundThread(Action action)
        {
            Task.Run(action);
        }

        public void InvokeInMainThread(Action action)
        {
            App.Current.Dispatcher.Invoke(action);
        }
    }
}