﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BookWiki.Core.Content;
using BookWiki.Core.Files.FileModels;
using BookWiki.Core.Files.FileSystemModels;
using BookWiki.Core.Files.PathModels;
using BookWiki.Core.Logging;
using BookWiki.Core.Utils.PropertyModels;

namespace BookWiki.Core.LibraryModels
{
    public class Library : ILibrary
    {
        private readonly Logger _logger = new Logger("Library");

        private readonly List<IContent> _changed;

        private readonly CachedValue<IContent[]> _content;
        private Timer _autosave;

        public Library(IPath root)
        {
            _changed = new List<IContent>();

            _content = new CachedValue<IContent[]>(() =>
            {
                var fs = new Files.FileSystemModels.FileSystem(root.FullPath);

                return fs.Contents.ToArray().Select(x =>
                {
                    if (x.Extension.Type == NodeType.Novel)
                    {
                        return (IContent)new EqualityNovel(new Novel(new ContentFolder(x)));
                    }

                    return null;
                }).Where(x => x != null).ToArray();
            });

            _autosave = new Timer(Callback, new object(), TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(30));
        }

        private void Callback(object state)
        {
            _logger.Info("Autosave elapsed.");

            Save();
        }

        public IContent[] Items => _content.Value;

        public void Update(IContent content)
        {
            lock (_changed)
            {
                _changed.RemoveAll(x => x.Source.EqualsTo(content.Source));
                _changed.Add(content);
            }
        }

        public void Save()
        {
            lock (_changed)
            {
                _logger.Info($"Saving {_changed.Count} items.");

                foreach (var content in _changed)
                {
                    if (content is INovel novel)
                    {
                        var file = new ContentFolder(content.Source);
                        file.Save(novel.Content);
                        file.Save(novel.Format);
                    }

                    if (content is IArticle article)
                    {
                        var file = new ContentFolder(content.Source);
                        file.Save(article.Content);
                    }
                }

                _changed.Clear();

                _logger.Info("Saved.");
            }
        }

        public IContent Load(IPath novelPath)
        {
            var item = Items.FirstOrDefault(x => x.Source.EqualsTo(novelPath));

            if (item == null)
            {
                _content.Invalidate();
            }

            return new EqualityNovel(new Novel(new ContentFolder(novelPath)));
        }
    }
}