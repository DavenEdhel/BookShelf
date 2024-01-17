namespace BookWiki.Presentation.Wpf.Models.Utilities
{
    using System.Collections.Generic;

    public class ThreadSafeList<T>
    {
        private readonly bool _ignoreDuplicates;
        private readonly List<T> _items = new List<T>();

        public ThreadSafeList(bool ignoreDuplicates = true)
        {
            this._ignoreDuplicates = ignoreDuplicates;
        }

        public void Add(T item)
        {
            lock (this._items)
            {
                if (this._ignoreDuplicates)
                {
                    if (this._items.Contains(item))
                    {
                        return;
                    }
                }

                this._items.Add(item);
            }
        }

        public void Clear()
        {
            lock (this._items)
            {
                this._items.Clear();
            }
        }

        public T[] GetLocalCopy()
        {
            T[] localCopy;

            lock (this._items)
            {
                localCopy = this._items.ToArray();
            }

            return localCopy;
        }

        public void Remove(T item)
        {
            lock (this._items)
            {
                this._items.Remove(item);
            }
        }
    }
}