namespace BookWiki.Core.FileSystem.FileModels
{
    public interface IFile
    {
        bool Exists { get; }

        void Save(string content);

        string Content { get; }

        string[] Lines { get; }
    }
}