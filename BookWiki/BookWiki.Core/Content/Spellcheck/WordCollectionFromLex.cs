using System;
using System.Linq;
using System.Threading.Tasks;
using BookWiki.Core.Logging;

namespace BookWiki.Presentation.Wpf.Models.SpellCheckModels
{
    public class WordCollectionFromLex : IMutableWordCollection
    {
        private const int _customWordsStartedFrom = 585005;

        private readonly string _lexPath;

        private readonly object _criticalSection = new object();

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
                lock (_criticalSection)
                {
                    _logger.Info($"Started loading {DateTime.Now.ToLongTimeString()}");

                    var all = _fileProvider.ReadAllLines(_lexPath);

                    var root = new WordsSubset(0, all);

                    _root = root;

                    IsLoaded = true;

                    _logger.Info($"Ended loading {DateTime.Now.ToLongTimeString()}");    
                }
            });
        }

        public bool IsFinalWord { get; } = false;

        public bool IsLoaded { get; private set; }

        public int LetterPosition { get; } = 0;

        public IWordCollection GetWordsWithLetterInPosition(char letter)
        {
            return _root.GetWordsWithLetterInPosition(letter);
        }

        public Task Learn(string newWord)
        {
            return Task.Run(() =>
            {
                lock (_criticalSection)
                {
                    _logger.Info($"Started learning {DateTime.Now.ToLongTimeString()}");

                    var all = _fileProvider.ReadAllLines(_lexPath).ToList();

                    newWord = newWord.ToLower();

                    if (all.Contains(newWord))
                    {
                        return;
                    }

                    _fileProvider.Append(_lexPath, newWord);

                    all.Add(newWord);

                    var root = new WordsSubset(0, all.ToArray());

                    _root = root;

                    _logger.Info($"Ended learning {DateTime.Now.ToLongTimeString()}");
                }
            });
        }
    }
}