using BookWiki.Core.Files.PathModels;
using BookWiki.Core.ViewModels;

namespace BookWiki.Presentation.Wpf.Models
{
    public class EditorState : IEditorState
    {
        private readonly NovelWindow _novel;

        public EditorState(NovelWindow novel)
        {
            _novel = novel;
        }

        public IRelativePath NovelPathToLoad => _novel.Novel;

        public int ScrollPosition => (int)_novel.Scroll.ContentVerticalOffset;

        public int LastCaretPosition => _novel.Rtb.Document.ContentStart.GetOffsetToPosition(_novel.Rtb.CaretPosition);

        public bool IsEditing => true;
    }
}