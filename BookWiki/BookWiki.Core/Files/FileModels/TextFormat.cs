using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BookWiki.Core.Utils.PropertyModels;
using Newtonsoft.Json;

namespace BookWiki.Core.Files.FileModels
{
    public class TextFormat : ISequence<ITextInfo>
    {
        private readonly ISequence<ITextInfo> _source;

        public static ISequence<ITextInfo> ParseFrom(string json, IText origin)
        {
            var format = JsonConvert.DeserializeObject<Scheme[]>(json) ?? new Scheme[0];

            var items = format.Select(x => new TextInfo(new LazySubstringText(new CachedValue<IText>(origin), x.Offset, x.Length), x.Style));

            return new EnumerableSequence<ITextInfo>(items, format.Length);
        }

        public TextFormat(ISequence<ITextInfo> source)
        {
            _source = source;
        }

        public string ToJson()
        {
            var format = this.Select(x => new Scheme()
                {
                    Offset = x.Range.Offset,
                    Length = x.Range.Length,
                    Style = x.Style
                })
                .ToArray();

            var content = JsonConvert.SerializeObject(format);

            return content;
        }

        public IEnumerator<ITextInfo> GetEnumerator()
        {
            return _source.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Progress Completion => _source.Completion;

        public class Scheme
        {
            public int Offset { get; set; }

            public int Length { get; set; }

            public TextStyle Style { get; set; }
        }
    }
}