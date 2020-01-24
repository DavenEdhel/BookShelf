using BookWiki.Core.Files.PathModels;

namespace BookWiki.Core.ViewModels
{
    public class MutableEditorState : IEditorState
    {
        public IRelativePath NovelPathToLoad { get; set; }

        public int ScrollPosition { get; set; } = 0;

        public int LastCaretPosition { get; set; } = 0;

        public bool IsEditing { get; set; } = false;
    }
}