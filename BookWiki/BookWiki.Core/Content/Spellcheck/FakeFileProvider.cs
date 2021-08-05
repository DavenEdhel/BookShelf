using System;

namespace BookWiki.Presentation.Wpf.Models.SpellCheckModels
{
    public class FakeFileProvider : IFileProvider
    {
        private readonly string[] _toProvide;

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
            throw new NotImplementedException();
        }
    }
}