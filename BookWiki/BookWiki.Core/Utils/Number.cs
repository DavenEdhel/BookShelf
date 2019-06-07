namespace BookWiki.Core.Utils
{
    public class Number
    {
        private readonly int _value;
        private readonly int? _minimum;
        private int? _maximum;
        private int? _result;

        private int Value
        {
            get
            {
                if (_result.HasValue)
                {
                    return _result.Value;
                }

                if (_maximum.HasValue && _minimum.HasValue && _maximum < _minimum)
                {
                    _maximum = _minimum;
                }

                if (_minimum != null && _value < _minimum.Value)
                {
                    _result = _minimum.Value;
                    return _result.Value;
                }

                if (_maximum != null && _value > _maximum.Value)
                {
                    _result = _maximum.Value;
                    return _result.Value;
                }

                _result = _value;
                return _result.Value;
            }
        }

        public Number(int value, int? minimum = null, int? maximum = null)
        {
            _value = value;
            _minimum = minimum;
            _maximum = maximum;
        }

        public Number(int index, string source) : this (index, 0, string.IsNullOrEmpty(source) ? 0 : (source.Length - 1))
        {

        }

        public static implicit operator int(Number i)
        {
            return i.Value;
        }
    }
}