using System;
using System.Drawing;
using BookWiki.Core.Utils.TextModels;

namespace BookWiki.Core.LifeSpellCheckModels
{
    public interface IHighlightCollection
    {
        void Highlight(ISubstring toHighlight, bool specialStyle, Action<string> onClick = null);

        void ScrollTo(ISubstring toScroll);

        void ClearHighlighting();
    }
}