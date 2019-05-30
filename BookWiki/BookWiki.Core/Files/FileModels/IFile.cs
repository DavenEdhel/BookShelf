namespace BookWiki.Core.FileSystem.FileModels
{
    public interface IFile
    {
        void Save(string content);

        string Content { get; }
    }
}