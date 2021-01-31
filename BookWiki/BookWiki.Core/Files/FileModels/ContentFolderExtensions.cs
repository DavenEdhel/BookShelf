namespace BookWiki.Core.FileSystem.FileModels
{
    public static class ContentFolderExtensions
    {
        public static void Save(this IContentFolder folder, IFormattedContent formattedContent)
        {
            folder.Save(formattedContent.Format);
            folder.Save(formattedContent.Content);
        }
    }
}