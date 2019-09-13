using BookWiki.Core.Utils.TextModels;

namespace BookWiki.Core.LifeSpellCheckModels
{
    public interface IErrorsCollection
    {
        void Add(IRange error);

        void RemoveAll();
    }
}