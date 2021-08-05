using BookWiki.Core.Utils.TextModels;

namespace BookWiki.Core.LifeSpellCheckModels
{
    public interface IErrorsCollectionV2
    {
        void Add(ISubstring error);

        void RemoveAll();
    }
}