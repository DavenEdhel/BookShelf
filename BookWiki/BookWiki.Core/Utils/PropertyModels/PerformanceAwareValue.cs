using System;

namespace BookWiki.Core
{
    public class PerformanceAwareValue<TOut> : IProperty<TOut>
    {
        private readonly string _outputDescription;
        private readonly Func<TOut> _getValue;

        public PerformanceAwareValue(IProperty<TOut> property, string outputDescription)
        {
            _outputDescription = outputDescription;
            _getValue = () => property.Value;
        }

        public TOut Value
        {
            get
            {
                var time = DateTime.Now;

                var value = _getValue();

                System.Diagnostics.Debug.WriteLine(_outputDescription + " " + (DateTime.Now - time).TotalMilliseconds);

                return value;
            }
        }
    }
}