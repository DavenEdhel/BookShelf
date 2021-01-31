using System;
using System.IO;
using System.Threading.Tasks;
using BookWiki.Core.Logging;

namespace BookWiki.Presentation.Wpf.Models.SpellCheckModels
{
    public class WordCollectionFromLex : IWordCollection
    {
        private readonly string _lexPath;

        private WordsSubset _root;

        private Logger _logger = new Logger("Lex");
        private readonly IFileProvider _fileProvider;

        public WordCollectionFromLex(string lexPath, IFileProvider fileProvider = null)
        {
            _lexPath = lexPath;
            _fileProvider = fileProvider ?? new FileSystemApiProvider();
        }

        public Task Load()
        {
            return Task.Run(() =>
            {
                _logger.Info($"Started loading {DateTime.Now.ToLongTimeString()}");

                var all = _fileProvider.ReadAllLines(_lexPath);

                _root = new WordsSubset(0, all);

                IsLoaded = true;

                _logger.Info($"Ended loading {DateTime.Now.ToLongTimeString()}");
            });

        }

        public bool IsFinalWord { get; } = false;

        public bool IsLoaded { get; private set; }

        public int LetterPosition { get; } = 0;

        public IWordCollection GetWordsWithLetterInPosition(char letter)
        {
            return _root.GetWordsWithLetterInPosition(letter);
        }
    }

    public interface IFileProvider
    {
        string[] ReadAllLines(string filePath);
    }

    public class FileSystemApiProvider : IFileProvider
    {
        public string[] ReadAllLines(string filePath)
        {
            return File.ReadAllLines(filePath);
        }
    }

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
    }
}