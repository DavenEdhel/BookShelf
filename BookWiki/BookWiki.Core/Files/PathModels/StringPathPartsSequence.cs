using System.Collections;
using System.Collections.Generic;
using System.IO;
using BookWiki.Core.Search;
using BookWiki.Core.Utils.PropertyModels;
using BookWiki.Core.Utils.TextModels;

namespace BookWiki.Core.Files.PathModels
{
    public class StringPathPartsSequence : IPartsSequence
    {
        private readonly IProperty<string> _path;
        private readonly ISequence<ITextRange> _sequence;

        public StringPathPartsSequence(IProperty<string> path, ISequence<ITextRange> sequence = null)
        {
            _path = path;
            _sequence = sequence ?? new RunOnceSequence<ITextRange>(new TextRangeSequence(new CachedValue<IText>(() => new StringText(path.Value)), new CachedValue<int>(() => path.Value.Length), Path.DirectorySeparatorChar));
        }

        public StringPathPartsSequence(string path) : this(new CachedValue<string>(path))
        {
        }

        public string FullPath => _path.Value;

        public IEnumerator<ITextRange> GetEnumerator()
        {
            return _sequence.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Progress Completion => _sequence.Completion;
    }
}