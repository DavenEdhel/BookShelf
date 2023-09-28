using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Keurig.IQ.Core.CrossCutting.Extensions;

namespace BookWiki.Core.Utils.TextModels
{
    public class PunctuationSeparatedEnumerationV2 : IEnumerable<ISubstring>
    {
        private readonly IEnumerable<OffsetText> _inlines;

        public PunctuationSeparatedEnumerationV2(IEnumerable<OffsetText> inlines)
        {
            _inlines = inlines;
        }

        public IEnumerator<ISubstring> GetEnumerator()
        {
            var incompletedIntervals = new List<IncompletedSubstring>();

            foreach (var i in _inlines)
            {
                var intervals = new IntervalsBetweenCharacters(i.Text, TextParts.NotALetterOrNumber).ToArray();

                var incompleted = intervals.Select(x => new IncompletedSubstring()
                {
                    Text = i.Text.Substring(x),
                    Start = x.Start + i.Offset,
                    End = x.End + i.Offset + 1
                }).ToArray();

                if (incompletedIntervals.Any())
                {
                    if ((incompleted.Any() == false || incompleted.First().Start != i.Offset) && incompletedIntervals.Last().End.HasValue == false)
                    {
                        var lastToResolve = incompletedIntervals.Last();
                        lastToResolve.End = lastToResolve.Start + lastToResolve.Text.Length + 1;
                    }
                }

                if (incompleted.Any())
                {
                    if (TextParts.NotALetterOrNumber.Contains(i.Text.Last()) == false && _inlines.Count() > 1 && i != _inlines.Last())
                    {
                        incompleted.Last().End = null;
                    }

                    incompletedIntervals.AddRange(incompleted);
                }
            }

            if (incompletedIntervals.Any() == false)
            {
                return new List<ISubstring>().GetEnumerator();
            }

            incompletedIntervals.Last().End = incompletedIntervals.Last().Start + incompletedIntervals.Last().Text.Length;

            var intervalsToResolve = incompletedIntervals.ToArray();
            var result = new List<ISubstring>();

            var resolvedIntervals = new List<IncompletedSubstring>();

            foreach (var incompletedSubstring in intervalsToResolve)
            {
                if (incompletedSubstring.End.HasValue)
                {
                    var last = resolvedIntervals.LastOrDefault();
                    if (last != null && last.End == null)
                    {
                        last.Text += incompletedSubstring.Text;
                        last.End = incompletedSubstring.End;
                    }
                    else
                    {
                        resolvedIntervals.Add(incompletedSubstring);
                    }

                    continue;
                }
                else
                {
                    var last = resolvedIntervals.LastOrDefault();

                    if (last == null || last.End != null)
                    {
                        last = new IncompletedSubstring();
                        last.Start = incompletedSubstring.Start;
                        last.Text = incompletedSubstring.Text;
                        last.End = null;
                        resolvedIntervals.Add(last);
                    }
                    else
                    {
                        last.Text += incompletedSubstring.Text;
                    }
                }
            }

            foreach (var resolvedInterval in resolvedIntervals)
            {
                result.Add(new Substring(resolvedInterval.Text, resolvedInterval.Start, resolvedInterval.End.Value));
            }

            return result.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private class IncompletedSubstring
        {
            public string Text { get; set; }

            public int Start { get; set; }

            public int? End { get; set; }
        }
    }

    public class PunctuationSeparatedEnumerationV3 : IEnumerable<ISubstring>
    {
        private readonly IEnumerable<OffsetText> _inlines;

        public PunctuationSeparatedEnumerationV3(IEnumerable<OffsetText> inlines)
        {
            _inlines = inlines;
        }

        public IEnumerator<ISubstring> GetEnumerator()
        {
            var incompletedIntervals = new List<IncompletedSubstring>();

            if (_inlines.Any() == false)
            {
                yield break;
            }

            var offset = _inlines.First().Offset;
            var entireText = _inlines.Select(x => x.Text).JoinStringsWithoutSkipping("");

            var intervals = new IntervalsBetweenCharacters(entireText, TextParts.NotALetterOrNumber).ToArray();

            var incompleted = intervals.Select(
                x => new Substring(
                    entireText.Substring(x),
                    x.Start + offset,
                    x.End + offset + 1
                )
            ).ToArray();

            foreach (var substring in incompleted)
            {
                yield return substring;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private class IncompletedSubstring
        {
            public string Text { get; set; }

            public int Start { get; set; }

            public int? End { get; set; }
        }
    }
}