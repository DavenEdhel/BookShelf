using BookWiki.Core.Utils.TextModels;

namespace BookWiki.Core
{
    public interface ITextInfo
    {
        IRange Range { get; }

        TextStyle Style { get; }
    }
}