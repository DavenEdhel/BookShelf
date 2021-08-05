namespace BookWiki.Core.Files.FileModels
{
    public class PageNumber
    {
        public const string Pages = "Pages";
        public const string AuthorLists = "ALs";
        public const string Characters = "Chars";
        public const string NotDiplay = "No Pages";

        public static readonly string[] PageModes = new string[]
        {
            Pages, AuthorLists, Characters, NotDiplay
        };
    }
}