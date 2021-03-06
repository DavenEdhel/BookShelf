﻿namespace BookWiki.Core.Files.PathModels
{
    public interface IPartsSequence : ISequence<ITextRange>
    {
        string FullPath { get; }
    }
}