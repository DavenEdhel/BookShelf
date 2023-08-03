using System.Linq;
using BookWiki.Core.Utils.TextModels;
using Keurig.IQ.Core.CrossCutting.Extensions;

namespace BookWiki.Core.Fb2Models
{
    public class WrapEachChapterInSection : IString
    {
        public WrapEachChapterInSection(string text)
        {
            var split = new SplitByTag(text, "title").ToArray();

            Value = split.Select(x => new WrappedText(x, "section").Value)
                .JoinStringsWithoutSkipping("");
        }

        public string Value { get; }
    }
}