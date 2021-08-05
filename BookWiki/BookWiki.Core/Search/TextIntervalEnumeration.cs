using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BookWiki.Core.Utils.PropertyModels;
using BookWiki.Core.Utils.TextModels;
using BookWiki.Presentation.Apple.Models.Utils;

namespace BookWiki.Core.Search
{
    public class TextIntervalEnumeration : IEnumerable<ITextRange>
    {
        private readonly IText _origin;
        private readonly IRange _frame;
        private readonly IEnumerable<IRange> _ranges;

        public TextIntervalEnumeration(IText origin, IRange frame, IEnumerable<IRange> ranges)
        {
            _origin = origin;
            _frame = frame;
            _ranges = ranges;
        }

        public IEnumerator<ITextRange> GetEnumerator()
        {
            var limits = new List<int>();
            limits.Add(_frame.Offset);
            limits.Add(_frame.Offset + _frame.Length);

            foreach (var textRange in _ranges)
            {
                limits.Add(textRange.Offset);
                limits.Add(textRange.Offset + textRange.Length);
            }

            var orderedLimits = limits.Where(x => x >= _frame.Start() && x <= _frame.End()).OrderBy(x => x).Distinct().ToArray();

            for (int i = 0; i < orderedLimits.Length - 1; i++)
            {
                yield return _origin.Substring(orderedLimits[i], orderedLimits[i + 1] - orderedLimits[i]);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}