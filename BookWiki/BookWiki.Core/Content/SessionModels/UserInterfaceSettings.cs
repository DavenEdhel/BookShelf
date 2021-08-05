namespace BookWiki.Core.Files.FileModels
{
    public class UserInterfaceSettings
    {
        public bool IsSideBarHidden { get; set; } = false;

        public bool IsScrollHidden { get; set; } = true;

        public bool IsSpellCheckOn { get; set; } = true;

        public int PageModeIndex { get; set; } = 0;
    }
}