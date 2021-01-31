using System.Threading.Tasks;

namespace BookWiki.Presentation.Wpf.Models.SpellCheckModels
{
    public interface IMutableWordCollection : IWordCollection
    {
        Task Learn(string newWord);
    }
}