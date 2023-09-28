using System.Linq;
using BookWiki.Core.Articles;
using BookWiki.Presentation.Wpf.Models.SpellCheckModels;

namespace BookWiki.Core.LifeSpellCheckModels
{
    public class SpellCheckV2 : ISpellCheckerV2
    {
        private readonly IWordCollection _wordCollection;
        private readonly ILex _exceptions;

        public SpellCheckV2(IWordCollection wordCollection, ILex exceptions)
        {
            _wordCollection = wordCollection;
            _exceptions = exceptions;
        }

        public bool IsCorrect(string word)
        {
            if (_wordCollection.IsLoaded == false)
            {
                return true;
            }

            if (_exceptions.Words.Contains(word.ToLowerInvariant()))
            {
                return true;
            }

            word = word.ToLower();

            var currentWords = _wordCollection;

            for (int i = 0; i < word.Length; i++)
            {
                var c = word[i];

                var nextWords = currentWords.GetWordsWithLetterInPosition(c);

                if (nextWords == null)
                {
                    return false;
                }

                if (i == word.Length - 1)
                {
                    return nextWords.IsFinalWord;
                }

                currentWords = nextWords;
            }

            return true;
        }
    }
}