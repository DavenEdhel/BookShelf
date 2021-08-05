namespace BookWiki.Presentation.Wpf.Models.Utilities
{
    using System;

    public interface IThreadInvoker
    {
        void InvokeInBackgroundThread(Action action);

        void InvokeInMainThread(Action action);
    }
}