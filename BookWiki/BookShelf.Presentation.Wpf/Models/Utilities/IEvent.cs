namespace BookWiki.Presentation.Wpf.Models.Utilities
{
    using System;

    public interface IEvent
    {
        void Listen(Action action);

        void ListenInMainThread(Action action);

        void Remove(Action action);

        void RemoveAll();
    }
}