using BookWiki.Core.Utils.TextModels;

namespace BookWiki.Core.LifeSpellCheckModels
{
    public interface IHighlightCollection
    {
        void Highlight(ISubstring toHighlight, bool specialStyle);

        void ScrollTo(ISubstring toScroll);

        void ClearHighlighting();
    }
}