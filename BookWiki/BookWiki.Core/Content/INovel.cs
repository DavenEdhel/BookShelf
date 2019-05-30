﻿namespace BookWiki.Core
{
    public interface INovel : IContent
    {
        ISequence<ITextInfo> Format { get; }
    }
}