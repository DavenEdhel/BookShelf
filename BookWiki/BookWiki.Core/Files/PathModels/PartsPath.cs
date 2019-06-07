using System.Collections.Generic;
using System.Linq;
using BookWiki.Core.Utils.TextModels;
using Keurig.IQ.Core.CrossCutting.Extensions;

namespace BookWiki.Core.Files.PathModels
{
    public class PartsPath : IPath
    {
        public PartsPath(IEnumerable<ITextRange> firstParts, IEnumerable<ITextRange> secondParts) : this(firstParts.CombineWith(secondParts))
        {
        }

        public PartsPath(IEnumerable<ITextRange> parts)
        {
            Parts = new PartsSequence(new EnumerableSequence<ITextRange>(parts));
            Name = new FileName(Parts);
            Extension = new Extension(Parts);
        }

        public IFileName Name { get; }
        public IExtension Extension { get; }

        public string FullPath => Parts.FullPath;

        public bool EqualsTo(IPath path)
        {
            var parts = path.Parts.ToArray();

            if (parts.Length != Parts.Count())
            {
                return false;
            }
            
            for (int i = 0; i < parts.Length; i++)
            {
                var origin = Parts.ElementAt(i);
                var part = parts[i];

                if (part.PlainText.Equals(origin.PlainText) == false)
                {
                    return false;
                }
            }

            return true;
        }

        public IPartsSequence Parts { get; }
    }
}