using System;
using System.Collections;
using System.Collections.Generic;
using BookWiki.Core.Files.FileModels;
using BookWiki.Core.Files.PathModels;
using BookWiki.Core.FileSystem.FileModels;
using BookWiki.Core.Utils.PropertyModels;
using BookWiki.Presentation.Wpf.Models;
using Keurig.IQ.Core.CrossCutting.Extensions;

namespace BookWiki.Core
{
    public class Article : IFormattedContent
    {
        private readonly IRelativePath _novelPath;
        private readonly IContentFolder _contentFolder;
        private readonly CachedValue<IText> _text;
        private readonly CachedValue<ISequence<ITextInfo>> _format;
        private readonly ArticleMetadata _metadata;

        public Article(IRelativePath novelPath, IRootPath root)
        {
            _novelPath = novelPath;
            _contentFolder = new ContentFolder(novelPath.AbsolutePath(root));
            _text = new CachedValue<IText>(() => _contentFolder.LoadText());
            _format = new CachedValue<ISequence<ITextInfo>>(() => new RunOnceSequence<ITextInfo>(_contentFolder.LoadFormat()));
            _metadata = new ArticleMetadata(novelPath.AbsolutePath(root));
        }

        public string Title => _contentFolder.Source.Name.PlainText;

        public IRelativePath Source => _novelPath;

        public ISequence<ITextInfo> Format => _format.Value;

        public IText Content => _text.Value;

        public string Name => _metadata.Name;

        public string[] NameVariations => _metadata.NameVariations;

        public string[] Tags => _metadata.Tags;

        public void Refresh()
        {
            _text.Invalidate();
            _format.Invalidate();
            _metadata.Invalidate();
        }
    }

    public class Novel : INovel, IFormattedContent, IComparable
    {
        private readonly IRelativePath _novelPath;
        private readonly IProperty<IText> _text;
        private readonly IProperty<IText> _comments;
        private readonly IProperty<ISequence<ITextInfo>> _format;
        private readonly IContentFolder _contentFolder;

        public Novel(IRelativePath novelPath, IRootPath root)
        {
            _novelPath = novelPath;
            _contentFolder = new ContentFolder(novelPath.AbsolutePath(root));
            _comments = new CachedValue<IText>(() => _contentFolder.LoadComments());
            _text = new CachedValue<IText>(() => _contentFolder.LoadText());
            _format = new CachedValue<ISequence<ITextInfo>>(() => new RunOnceSequence<ITextInfo>(_contentFolder.LoadFormat()));
        }

        public IText Content => _text.Value;

        public string Title => _contentFolder.Source.Name.PlainText;

        public IRelativePath Source => _novelPath;

        public ISequence<ITextInfo> Format => _format.Value;

        public IText Comments => _comments.Value;

        public int CompareTo(object obj)
        {
            if (obj is Novel n)
            {
                if (Source.EqualsTo(n.Source))
                {
                    return 0;
                }

                var xName = new NovelTitleShort(Source);
                var yName = new NovelTitleShort(n.Source);

                return xName.CompareTo(yName);
            }

            return -1;

            
        }
    }
}