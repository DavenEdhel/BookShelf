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

        public Fb2BookContent(List<IAbsolutePath> chapters)
        {
            _chapters = chapters;
        }

        public Func<IAbsolutePath, IString> Chapter { get; set; } = x => new Fb2Chapter(x);

        public string Value => string.Join("\n", _chapters.Select(x => Chapter(x).Value));
    }
}