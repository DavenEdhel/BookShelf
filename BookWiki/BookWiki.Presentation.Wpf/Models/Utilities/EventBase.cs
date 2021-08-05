namespace BookWiki.Presentation.Wpf.Models.Utilities
{
    using System;

    public abstract class EventBase<T>
    {
        private readonly ThreadSafeList<T> _backgroundActions = new ThreadSafeList<T>();
        private readonly ThreadSafeList<T> _mainThreadActions = new ThreadSafeList<T>();
        private readonly IThreadInvoker _threadInvoker;

        protected EventBase(IThreadInvoker threadInvoker = null)
        {
            this._threadInvoker = threadInvoker.SelfOrResolve();
        }

        public DateTime LastRaisedTime { get; private set; } = DateTime.Now;

        public void Listen(T action)
        {
            this._backgroundActions.Add(action);
        }

        public void ListenInMainThread(T action)
        {
            this._mainThreadActions.Add(action);
        }

        public void Remove(T action)
        {
            this._mainThreadActions.Remove(action);
            this._backgroundActions.Remove(action);
        }

        public void RemoveAll()
        {
            this._mainThreadActions.Clear();
            this._backgroundActions.Clear();
        }

        protected void Raise(Func<T, Operation> toOperation)
        {
            try
            {
                var mainThreadActions = this._mainThreadActions.GetLocalCopy();

                if (mainThreadActions.Length > 0)
                {
                    this._threadInvoker.InvokeInMainThread(
                        () =>
                        {
                            foreach (var action in mainThreadActions)
                            {
                                toOperation(action)
                                    .FailSafe()
                                    .Execute();
                            }
                        });
                }

                var backgroundThreadActions = this._backgroundActions.GetLocalCopy();

                foreach (var action in backgroundThreadActions)
                {
                    toOperation(action)
                        .FailSafe()
                        .Execute();
                }
            }
            finally
            {
                this.LastRaisedTime = DateTime.Now;
            }
        }
    }
}