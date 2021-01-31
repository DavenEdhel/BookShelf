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
                    if (TextParts.NotALetterOrNumber.Contains(i.Text.Last()) == false && i != _inlines.Last())
                    {
                        incompleted.Last().End = null;
                    }

                    incompletedIntervals.AddRange(incompleted);
                }
            }

            var intervalsToResolve = incompletedIntervals.ToArray();
            var result = new List<ISubstring>();

            for (int i = 0; i < intervalsToResolve.Length; i++)
            {
                if (result.Any() && result.Max(x => x.EndIndex) >= intervalsToResolve[i].Start)
                {
                    continue;
                }

                if (intervalsToResolve[i].End.HasValue)
                {
                    result.Add(new Substring(intervalsToResolve[i].Text, intervalsToResolve[i].Start, intervalsToResolve[i].End.Value));
                    continue;
                }

                if (intervalsToResolve[i].End.HasValue == false)
                {
                    var inTheMiddle = intervalsToResolve.Where(x => x.Start > intervalsToResolve[i].Start && x.End.HasValue == false).SelfOrEmpty();

                    var toResolveEndFrom = intervalsToResolve.FirstOrDefault(x => x.Start > intervalsToResolve[i].Start && x.End.HasValue);

                    if (toResolveEndFrom == null)
                    {
                        result.Add(new Substring(intervalsToResolve[i].Text, intervalsToResolve[i].Start, intervalsToResolve[i].Start + intervalsToResolve[i].Text.Length + 1));
                    }
                    else
                    {
                        var toResolveTextFrom = inTheMiddle.And(new IncompletedSubstring[] {toResolveEndFrom}).ToArray();

                        var text = intervalsToResolve[i].Text + toResolveTextFrom.Select(x => x.Text).JoinStringsWithoutSkipping("");

                        result.Add(new Substring(text, intervalsToResolve[i].Start, toResolveEndFrom.End.Value));
                    }
                }
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
}