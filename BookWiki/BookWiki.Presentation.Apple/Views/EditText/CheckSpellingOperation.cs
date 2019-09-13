using BookWiki.Core;
using BookWiki.Core.LifeSpellCheckModels;
using BookWiki.Core.Utils;
using BookWiki.Core.Utils.TextModels;
using UIKit;

namespace BookWiki.Presentation.Apple.Views.Controls
{
    public class CheckSpellingOperation : ISpellChecker
    {
        public ISequence<IRange> Execute(string text, IRange range)
        {
            return new MisspelledWordsSequence(new UITextChecker(), text, range);
        }
    }
}