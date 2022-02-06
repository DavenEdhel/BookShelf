using Keurig.IQ.Core.CrossCutting.Extensions;

namespace BookWiki.Core.Files.FileModels
{
    public class RightSideBarSettings
    {
        public bool IsVisible { get; set; } = false;

        public bool IsCommentBarVisible { get; set; } = false;

        public bool IsConsoleBarHidden { get; set; } = false;

        public RightSideBarSettings Clone()
        {
            return MemberwiseClone().CastTo<RightSideBarSettings>();
        }

        public bool EqualsTo(RightSideBarSettings settings)
        {
            return settings.IsVisible == IsVisible &&
                   settings.IsCommentBarVisible == IsCommentBarVisible &&
                   settings.IsConsoleBarHidden == IsConsoleBarHidden;
        }
    }
}