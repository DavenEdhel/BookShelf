namespace BookWiki.Presentation.Wpf.Models.Utilities
{
    using System;

    public class DependenciesContainer
    {
        public static readonly DependenciesContainer Instance = new DependenciesContainer();

        private IThreadInvoker _threadInvoker;

        public T Resolve<T>()
        {
            switch (default(T))
            {
                case IThreadInvoker threadInvoker:
                    return (T)(this._threadInvoker = this._threadInvoker ?? new ThreadInvoker());
                default:
                    throw new Exception("Cannot resolve type");
            }
        }
    }
}