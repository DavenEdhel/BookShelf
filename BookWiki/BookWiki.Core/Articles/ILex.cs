using System.Collections.Generic;

namespace BookWiki.Core.Articles
{
    public interface ILex
    {
        IEnumerable<string> Words { get; }
    }
}