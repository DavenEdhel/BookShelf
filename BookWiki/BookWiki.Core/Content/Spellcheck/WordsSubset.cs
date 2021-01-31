using System;
using System.Linq;

namespace BookWiki.Presentation.Wpf.Models.SpellCheckModels
{
    public class WordsSubset : IWordCollection
    {
        private static int ToIndex(char c)
        {
            switch (c)
            {
                case 'а': return 0;
                case 'б': return 1;
                case 'в': return 2;
                case 'г': return 3;
                case 'д': return 4;
                case 'е': return 5;
                case 'ё': return 6;
                case 'ж': return 7;
                case 'з': return 8;
                case 'и': return 9;
                case 'й': return 10;
                case 'к': return 11;
                case 'л': return 12;
                case 'м': return 13;
                case 'н': return 14;
                case 'о': return 15;
                case 'п': return 16;
                case 'р': return 17;
                case 'с': return 18;
                case 'т': return 19;
                case 'у': return 20;
                case 'ф': return 21;
                case 'х': return 22;
                case 'ц': return 23;
                case 'ч': return 24;
                case 'ш': return 25;
                case 'щ': return 26;
                case 'ъ': return 27;
                case 'ы': return 28;
                case 'ь': return 29;
                case 'э': return 30;
                case 'ю': return 31;
                case 'я': return 32;

                default: return -1;
            }
        }

        private static char ToChar(int c)
        {
            switch (c)
            {
                case 0: return 'а';  
                case 1: return 'б';  
                case 2: return 'в';  
                case 3: return 'г';  
                case 4: return 'д';  
                case 5: return 'е';  
                case 6: return 'ё';  
                case 7: return 'ж';  
                case 8: return 'з';  
                case 9: return 'и';  
                case 10: return 'й';
                case 11: return 'к';
                case 12: return 'л';
                case 13: return 'м';
                case 14: return 'н';
                case 15: return 'о';
                case 16: return 'п';
                case 17: return 'р';
                case 18: return 'с';
                case 19: return 'т';
                case 20: return 'у';
                case 21: return 'ф';
                case 22: return 'х';
                case 23: return 'ц';
                case 24: return 'ч';
                case 25: return 'ш';
                case 26: return 'щ';
                case 27: return 'ъ';
                case 28: return 'ы';
                case 29: return 'ь';
                case 30: return 'э';
                case 31: return 'ю';
                case 32: return 'я';

                default: throw new ArgumentOutOfRangeException();
            }
        }

        private IWordCollection[] _characters = new IWordCollection[33];
        private readonly int _position;

        public WordsSubset(int position, string[] items)
        {
            _position = position;

            IsFinalWord = items.Any(x => x.Length == position);

            for (int i = 0; i < 33; i++)
            {
                var cToCheck = ToChar(i);

                var itemsWithCharacter =
                    items.Where(x => x.Length > position && x[position] == cToCheck).ToArray();

                if (itemsWithCharacter.Any())
                {
                    _characters[i] = new WordsSubset(position + 1, itemsWithCharacter);
                }
            }
        }

        public bool IsFinalWord { get; }

        public bool IsLoaded { get; } = true;

        public int LetterPosition => _position;

        public IWordCollection GetWordsWithLetterInPosition(char letter)
        {
            var index = ToIndex(letter);

            if (index == -1)
            {
                return null;
            }

            return _characters[index];
        }
    }
}