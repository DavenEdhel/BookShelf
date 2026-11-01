using BookWiki.Core.Search;
using BookWiki.Core.Utils.TextModels;
using Keurig.IQ.Core.CrossCutting.Extensions;
using System.Linq;

namespace BookWiki.Core.Fb2Models
{
    public class WrappedText : IString
    {
        public WrappedText(string text, string tag)
        {
            Tag = tag;
            Text = text;
        }

        public WrappedText(IString text, string tag)
        {
            Tag = tag;
            Text = text.Value;
        }

        public string Tag { get; } = "empty";

        public string Text { get; } = string.Empty;

        public string Value => $"<{Tag}>{Text}</{Tag}>\n";
    }

    public class ConfigurablyClearedText : IString
    {
        private readonly string text;
        private readonly bool withoutComments;

        public ConfigurablyClearedText(string text, bool withoutComments)
        {
            this.text = text;
            this.withoutComments = withoutComments;
        }

        public string Value
        {
            get
            {
                if (withoutComments)
                {
                    return new ClearedText(text).Value;
                }
                else
                {
                    return text;
                }
            }
        }
    }

    public class ClearedText : IString
    {
        private readonly string text;

        public ClearedText(string text)
        {
            this.text = text;
        }

        public string Value
        {
            get 
            {
                var result = new TextRangeSequence(text, '*').As(out var range)
                    .Select((x,index) => (x, index)).As(out var selected)
                    .Where(i => i.index%2 == 0).As(out var filtered)
                    .Select(i => i.x.PlainText)
                    .ToArray().As(out var plain)
                    .JoinStringsWithoutSkipping("");

                return result;
            }
        }
    }
}