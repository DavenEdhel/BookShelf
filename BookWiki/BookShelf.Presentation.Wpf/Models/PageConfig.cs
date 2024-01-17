using System;
using BookWiki.Core.Files.FileModels;
using BookWiki.Core.Utils;
using BookWiki.Presentation.Wpf.Models.Utilities;

namespace BookWiki.Presentation.Wpf.Models
{
    public class PageConfig
    {
        public PageConfig(SessionContext session)
        {
            Current = session.InterfaceSettings;
        }

        public UserInterfaceSettings Current { get; private set; }

        public event Action<UserInterfaceSettings> Changed = delegate(UserInterfaceSettings settings) { };

        public void SetDisplayMode(string mode)
        {
             Current.PageModeIndex = PageNumber.PageModes.IndexOf(x => x == mode);
             Changed(Current);
        }

        public void SetScrollVisibility(bool isVisible)
        {
            Current.IsScrollHidden = !isVisible;
            Changed(Current);
        }

        public void SetSpellcheckAvailability(bool isAvailable)
        {
            Current.IsSpellCheckOn = isAvailable;
            Changed(Current);
        }
    }
}