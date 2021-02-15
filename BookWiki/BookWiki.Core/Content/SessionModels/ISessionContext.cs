using System.Collections.Generic;
using BookWiki.Core.ViewModels;

namespace BookWiki.Core.Files.FileModels
{
    public interface ISessionContext
    {
        public IEnumerable<IEditorState> OpenedContentTabs { get; }

        public IEnumerable<IQuery> OpenedQueriesTabs { get; }

        public UserInterfaceSettings InterfaceSettings { get; }
    }
}