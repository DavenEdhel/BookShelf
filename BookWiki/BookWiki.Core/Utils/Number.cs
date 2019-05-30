using PureOop;

namespace BookWiki.Core.Utils
{
    public class Number
    {
        private readonly int _value;
        private readonly int? _minimum;
        private int? _maximum;

        [JustOnce]
        public int Value
        {
            get
            {
                if (_maximum.HasValue && _minimum.HasValue && _maximum < _minimum)
                {
                    _maximum = _minimum;
                }

                if (_minimum != null && _value < _minimum.Value)
                {
                    return _minimum.Value;
                }

                if (_maximum != null && _value > _maximum.Value)
                {
                    return _maximum.Value;
                }

                return _value;
            }
        }

        public Number(int value, int? minimum = null, int? maximum = null)
        {
            _value = value;
            _minimum = minimum;
            _maximum = maximum;
        }
    }
}