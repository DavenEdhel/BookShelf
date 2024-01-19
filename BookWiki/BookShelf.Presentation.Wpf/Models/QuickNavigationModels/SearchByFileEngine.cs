using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Subjects;
using System.Windows.Documents;
using BookWiki.Core;
using BookWiki.Core.Files.FileSystemModels;
using BookWiki.Core.Files.PathModels;

namespace BookWiki.Presentation.Wpf.Models.QuickNavigationModels
{
    public class SearchByFileEngine
    {
        private bool _isCacheReady = false;

        private List<KeyValuePair<string[], IFileSystemNode>> _parsedData;

        public IEnumerable<IFileSystemNode> Execute(string query)
        {
            if (query.Length > 2)
            {
                EnsureLeafsParsed();

                var searchResults = new List<KeyValuePair<IFileSystemNode, int>>();

                var queryParts = query.ToLowerInvariant().Split(' ');

                foreach (var keyValuePair in _parsedData)
                {
                    var score = 0;

                    foreach (var s in keyValuePair.Key)
                    {
                        foreach (var q in queryParts)
                        {
                            if (s.Contains(q))
                            {
                                score++;
                            }
                        }
                    }

                    searchResults.Add(new KeyValuePair<IFileSystemNode, int>(keyValuePair.Value, score));
                }

                return searchResults.Where(x => x.Value > 0).OrderByDescending(x => x.Value).Select(x => x.Key).ToArray();
            }

            return new IFileSystemNode[0];
        }

        public void InvalidateCache()
        {
            _isCacheReady = false;
        }

        private void EnsureLeafsParsed()
        {
            if (_isCacheReady == false)
            {
                _parsedData = new List<KeyValuePair<string[], IFileSystemNode>>();

                foreach (var fileSystemNode in BooksApplication.Instance.Root.GetAllLeafs())
                {
                    var parts = new NovelTitle(fileSystemNode.Path).PlainText.ToLowerInvariant().Split(' ').ToArray();

                    _parsedData.Add(new KeyValuePair<string[], IFileSystemNode>(parts, fileSystemNode));
                }

                _isCacheReady = true;
            }
        }
    }

    public class SearchMapEngine
    {
        private bool _isCacheReady = false;

        private List<KeyValuePair<string[], IFileSystemNode>> _parsedData;

        public IEnumerable<IFileSystemNode> Execute(string query)
        {
            InvalidateCache();

            EnsureLeafsParsed();

            var searchResults = new List<KeyValuePair<IFileSystemNode, int>>();

            var queryParts = query.ToLowerInvariant().Split(' ');

            foreach (var keyValuePair in _parsedData)
            {
                var score = 0;

                foreach (var s in keyValuePair.Key)
                {
                    foreach (var q in queryParts)
                    {
                        if (s.Contains(q))
                        {
                            score++;
                        }
                    }
                }

                searchResults.Add(new KeyValuePair<IFileSystemNode, int>(keyValuePair.Value, score));
            }

            return searchResults.Where(x => x.Value > 0).OrderByDescending(x => x.Value).Select(x => x.Key).ToArray();
        }

        public void InvalidateCache()
        {
            _isCacheReady = false;
        }

        private void EnsureLeafsParsed()
        {
            if (_isCacheReady == false)
            {
                _parsedData = new List<KeyValuePair<string[], IFileSystemNode>>();

                foreach (var fileSystemNode in new FileSystemNode(BooksApplication.Instance.Maps.MapsRoot.FullPath).GetAllLeafs())
                {
                    var parts = new NovelTitle(fileSystemNode.Path).PlainText.ToLowerInvariant().Split(' ').ToArray();

                    _parsedData.Add(new KeyValuePair<string[], IFileSystemNode>(parts, fileSystemNode));
                }

                _isCacheReady = true;
            }
        }
    }
}