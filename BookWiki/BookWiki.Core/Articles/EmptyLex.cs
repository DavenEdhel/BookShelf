using System;
using System.Collections.Generic;

namespace BookWiki.Core.Articles
{
    public class EmptyLex : ILex
    {
        public IEnumerable<string> Words => Array.Empty<string>();
    }
}