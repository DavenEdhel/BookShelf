using System;
using BookWiki.Core.Files.PathModels;
using BookWiki.Core.FileSystem.FileModels;
using BookWiki.Core.FileSystem.PathModels;
using BookWiki.Core.Utils.PropertyModels;
using Keurig.IQ.Core.CrossCutting.Extensions;

namespace BookWiki.Core
{
    public class NovelFake : INovel
    {
        public IText Content { get; set; }
        public string Title { get; set; }
        public IPath Source { get; set; }
        public ISequence<ITextInfo> Format { get; set; }
    }

    public class Novel : INovel
    {
        private readonly IProperty<IText> _text;
        private readonly IProperty<ISequence<ITextInfo>> _format;
        private IContentFolder _contentFolder;

        public Novel(IContentFolder contentFolder)
        {
            _contentFolder = contentFolder;

            _text = new CachedValue<IText>(() => _contentFolder.LoadText());
            _format = new CachedValue<ISequence<ITextInfo>>(() => new RunOnceSequence<ITextInfo>(_contentFolder.LoadFormat()));
        }

        public IText Content => _text.Value;

        public string Title => _contentFolder.Source.Name.PlainText;

        public IPath Source => _contentFolder.Source;

        public ISequence<ITextInfo> Format => _format.Value;
    }
}