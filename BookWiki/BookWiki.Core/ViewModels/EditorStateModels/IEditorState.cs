using BookWiki.Core.Files.PathModels;

namespace BookWiki.Core.ViewModels
{
    public interface IEditorState
    {
        IRelativePath NovelPathToLoad { get; }

        int ScrollPosition { get; }

        int LastCaretPosition { get; }

        bool IsEditing { get; }
    }
}