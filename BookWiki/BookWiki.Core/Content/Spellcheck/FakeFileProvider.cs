using System;
using System.Collections.Generic;
using BookWiki.Core.Utils;

namespace BookWiki.Presentation.Wpf.Models.SpellCheckModels
{
    public class FakeFileProvider : IFileProvider
    {
        private string[] _toProvide;

        public FakeFileProvider(string[] toProvide)
        {
            _toProvide = toProvide;
        }

        public string[] ReadAllLines(string filePath)
        {
            return _toProvide;
        }

        public void Append(string lexPath, string newWord)
        {
            var items = new List<string>(_toProvide);
            items.Add(newWord);

            _toProvide = items.ToArray();
        }
    }
}