using System;
using BookWiki.Core.Files.FileModels;

namespace BookWiki.Presentation.Wpf.Models
{
    public class RightSideBarConfig
    {
        public RightSideBarConfig(SessionContext session)
        {
            Current = session.InterfaceSettings.RightSideBar ?? new RightSideBarSettings();
        }

        public RightSideBarSettings Current { get; private set; }

        public event Action<RightSideBarSettings> Changed = delegate (RightSideBarSettings settings) { };

        public void Change(Action<RightSideBarSettings> change)
        {
            var copy = Current.Clone();

            change(copy);

            if (copy.EqualsTo(Current) == false)
            {
                Current = copy;

                Changed(Current);
            }
        }
    }
}