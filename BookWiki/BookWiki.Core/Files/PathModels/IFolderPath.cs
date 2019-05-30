namespace BookWiki.Core.FileSystem.PathModels
{
    public interface IFolderPath
    {
        ISequence<IFolderPath> Folders { get; }

        ISequence<IContentPath> Contents { get; }
    }
}