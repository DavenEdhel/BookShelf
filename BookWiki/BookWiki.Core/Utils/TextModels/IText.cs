using System.Collections.Generic;
using System.Linq;

namespace BookWiki.Core
{
    public interface IText
    {
        ITextRange Substring(int offset, int length);

        int Length { get; }
        
        string PlainText { get; }
    }
}