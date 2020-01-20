using BookWiki.Core.Files.PathModels;
using BookWiki.Core.ViewModels;
using BookWiki.Presentation.Apple.Models;
using BookWiki.Presentation.Apple.Views.Common;

namespace BookWiki.Presentation.Apple.Views.Controls
{
    public class NovelViewEditorState : IEditorState
    {
        private readonly NovelView _view;
        private readonly EditTextView _edit;

        public NovelViewEditorState(NovelView view, EditTextView edit)
        {
            _view = view;
            _edit = edit;
        }

        public IRelativePath NovelPathToLoad => _view.Source;
        public int ScrollPosition => (int) _edit.ContentOffset.Y;
        public int LastCaretPosition => (int)_edit.CursorPosition;
        public bool IsEditing => (_view.IsActive && Application.Instance.IsInEditMode) || _view.WasFocused;
    }
}