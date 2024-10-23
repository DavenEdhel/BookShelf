using System.Linq;

namespace BookWiki.Core.Utils.TextModels
{
    public interface IString
    {
        string Value { get; }
    }

    public class EmptyString : IString
    {
        public string Value { get; } = string.Empty;
    }

    public class WordMainPart : IString 
    {
        public WordMainPart(string word)
        {
            if (string.IsNullOrEmpty(word))
            {
                Value = "";
            }
            else if (new CharacterInfo(word.Last()).IsVowel)
            {
                Value = word.Substring(0, word.Length - 1);
            }
            else
            {
                Value = word;
            }

        }

        public string Value { get; }
    }

    public class WordEnding : IString
    {
        public WordEnding(string word)
        {
            if (new CharacterInfo(word.Last()).IsVowel)
            {
                Value = word.Last().ToString();
            }
            else
            {
                Value = string.Empty;
            }
        }

        public string Value { get; }
    }

    public class CharacterInfo
    {
        private readonly char _c;

        public CharacterInfo(char c)
        {
            _c = c;

            if (new char[]
                {
                    'а', 'о', 'у', 'ы', 'и', 'е', 'ю', 'ё', 'я', 'э'
                }.Contains(_c))
            {
                IsVowel = true;
            }
        }

        public bool IsVowel { get; }
    }
}