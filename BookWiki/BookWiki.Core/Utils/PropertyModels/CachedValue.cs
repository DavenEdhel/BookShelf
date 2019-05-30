using System;

namespace BookWiki.Core.Utils.PropertyModels
{
    public class CachedValue<TOut> : IProperty<TOut>
    {
        private readonly Func<TOut> _getValue;
        private TOut _cache;
        private bool _isRead;

        public CachedValue(Func<TOut> getValue)
        {
            _getValue = getValue;
        }

        public CachedValue(IProperty<TOut> property)
        {
            _getValue = () => property.Value;
        }

        public CachedValue(TOut value)
        {
            _getValue = () => value;
        }

        public TOut Value
        {
            get
            {
                if (_isRead)
                {
                    return _cache;
                }

                _cache = _getValue();
                _isRead = true;

                return _cache;
            }
        }

        public void Invalidate()
        {
            _isRead = false;
        }
    }
}