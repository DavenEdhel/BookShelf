using System;
using System.Collections.Generic;
using System.Linq;
using BookWiki.Core.Files.PathModels;
using BookWiki.Core.Utils.TextModels;

namespace BookWiki.Core.Fb2Models
{
    public class Fb2BookContent : IString
    {
        private readonly List<IAbsolutePath> _chapters;
        private readonly bool withoutComments;

        public Fb2BookContent(List<IAbsolutePath> chapters, bool withoutComments = false)
        {
            _chapters = chapters;
            this.withoutComments = withoutComments;

            Chapter = x => new Fb2Chapter(x, withoutComments);
        }

        public Func<IAbsolutePath, IString> Chapter { get; set; }

        public string Value => string.Join("\n", _chapters.Select(x => Chapter(x).Value));
    }
}