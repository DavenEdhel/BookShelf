using BookWiki.Core.Utils.PropertyModels;

namespace BookWiki.Core
{
    public class Progress
    {
        public string ProgressPercentage
        {
            get
            {
                var progress = (_current /  _max.Value) * 100f;

                return progress.ToString("0.00") + "%";
            }
        }

        private float _current;
        private readonly IProperty<float> _max;

        public Progress(int maxValue) : this(0, maxValue)
        {
        }

        public Progress(int current, int maxValue)
        {
            _current = current;
            _max = new CachedValue<float>(maxValue);
        }

        public Progress(IProperty<int> maxValue)
        {
            _current = 0;
            _max = new CachedValue<float>(() => maxValue.Value);
        }

        public void Reset()
        {
            _current = 0;
        }

        public void Change(int current)
        {
            _current = current;
        }

        public void Increment()
        {
            _current += 1;
        }

        public void MarkCompleted()
        {
            _current = _max.Value;
        }
    }
}