namespace BookWiki.Core.Files.PathModels
{
    public class FileNameWithoutExtension : IFileName
    {
        public FileNameWithoutExtension(string justName)
        {
            PlainText = justName;
        }

        public string PlainText { get; }
    }
}