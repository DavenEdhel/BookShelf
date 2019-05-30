using System;
using BookWiki.Core;
using CoreFoundation;
using Foundation;

namespace BookWiki.Presentation.Apple.Models.Utils
{
    public class ThreadSafeProperty<T> : IProperty<T>
    {
        private readonly Func<T> _getValue;

        public T Value
        {
            get
            {
                if (NSThread.Current.IsMainThread)
                {
                    return _getValue();
                }

                T result = default(T);

                DispatchQueue.MainQueue.DispatchSync(() => { result = _getValue(); });

                return result;
            }
        }

        public ThreadSafeProperty(Func<T> getValue)
        {
            _getValue = getValue;
        }
    }
}