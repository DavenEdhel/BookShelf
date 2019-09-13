using BookWiki.Core.Utils.TextModels;

namespace BookWiki.Core.LifeSpellCheckModels
{
    public interface ISpellChecker
    {
        ISequence<IRange> Execute(string text, IRange range);
    }
}