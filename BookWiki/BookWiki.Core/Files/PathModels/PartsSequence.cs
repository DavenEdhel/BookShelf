using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BookWiki.Core.Files.PathModels
{
    public class PartsSequence : IPartsSequence
    {
        private readonly ISequence<ITextRange> _parts;

        public PartsSequence(ISequence<ITextRange> parts)
        {
            _parts = parts;
        }

        public string FullPath => string.Join(string.Empty + Path.DirectorySeparatorChar, _parts.Select(x => x.PlainText).ToArray());

        public IEnumerator<ITextRange> GetEnumerator()
        {
            return _parts.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Progress Completion => _parts.Completion;
    }
}