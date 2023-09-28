using System;
using System.Collections;
using System.Collections.Generic;

namespace BookWiki.Core.Fb2Models
{
    public class SplitByWord : IEnumerable<string>
    {
        private readonly string _text;
        private readonly string _word;

        public SplitByWord(string text, string word)
        {
            _text = text;
            _word = word;
        }

        public IEnumerator<string> GetEnumerator()
        {
            var parts = _text.Split(new[] {_word}, StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in parts)
            {
                yield return _word + part;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class SplitByTag : IEnumerable<string>
    {
        private readonly string _text;
        private readonly string _tag;

        public SplitByTag(string text, string tag)
        {
            _text = text;
            _tag = tag;
        }

        public IEnumerator<string> GetEnumerator()
        {
            var parts = _text.Split(new[] { new OpenTag(_tag).ToString() }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in parts)
            {
                if (part.Contains(new CloseTag(_tag).ToString()))
                {
                    yield return new OpenTag(_tag) + part;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class OpenTag
    {
        private readonly string _tag;

        public OpenTag(string tag)
        {
            _tag = tag;
        }

        public override string ToString()
        {
            return $"<{_tag}>";
        }
    }

    public class CloseTag
    {
        private readonly string _tag;

        public CloseTag(string tag)
        {
            _tag = tag;
        }

        public override string ToString()
        {
            return $"</{_tag}>";
        }
    }
}