using System;
using BookWiki.Core.Files.FileModels;
using BookWiki.Core.Files.PathModels;
using BookWiki.Core.FileSystem.FileModels;
using BookWiki.Core.Utils.PropertyModels;
using Keurig.IQ.Core.CrossCutting.Extensions;

namespace BookWiki.Core
{
    public class Novel : INovel, IFormattedContent
    {
        private readonly IRelativePath _novelPath;
        private readonly IProperty<IText> _text;
        private readonly IProperty<ISequence<ITextInfo>> _format;
        private readonly IContentFolder _contentFolder;

        public Novel(IRelativePath novelPath, IRootPath root)
        {
            _novelPath = novelPath;
            _contentFolder = new ContentFolder(novelPath.AbsolutePath(root));

            _text = new CachedValue<IText>(() => _contentFolder.LoadText());
            _format = new CachedValue<ISequence<ITextInfo>>(() => new RunOnceSequence<ITextInfo>(_contentFolder.LoadFormat()));
        }

        public IText Content => _text.Value;

        public string Title => _contentFolder.Source.Name.PlainText;

        public IRelativePath Source => _novelPath;

        public ISequence<ITextInfo> Format => _format.Value;
    }
}