namespace BookWiki.Core.FileSystem.PathModels
{
    public interface IContentPath
    {
        ISequence<IFilePath> Files { get; }
    }
}