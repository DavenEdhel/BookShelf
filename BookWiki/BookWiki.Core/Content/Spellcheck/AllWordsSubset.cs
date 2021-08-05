namespace BookWiki.Presentation.Wpf.Models.SpellCheckModels
{
    public class AllWordsSubset : IWordCollection
    {
        public bool IsFinalWord { get; } = true;

        public bool IsLoaded { get; } = true;

        public int LetterPosition { get; } = 0;

        public IWordCollection GetWordsWithLetterInPosition(char letter)
        {
            return new AllWordsSubset();
        }
    }
}