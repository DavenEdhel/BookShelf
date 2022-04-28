using BookWiki.Core.Utils;

namespace BookWiki.Presentation.Wpf.Models.SpellCheckModels
{
    public interface IWordCollection
    {
        bool IsFinalWord { get; }

        bool IsLoaded { get; }

        int LetterPosition { get; }

        IWordCollection GetWordsWithLetterInPosition(char letter);

        void Append(string item);
    }
}