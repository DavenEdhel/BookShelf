using System.Collections.Generic;
using System.Linq;
using BookWiki.Core.Utils.TextModels;

namespace BookWiki.Core
{
    public interface IText
    {
        ITextRange Substring(int offset, int length);

        int Length { get; }
        
        string PlainText { get; }
    }
}