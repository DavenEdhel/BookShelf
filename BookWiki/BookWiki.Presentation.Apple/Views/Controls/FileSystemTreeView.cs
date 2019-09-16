using System;
using System.Linq;
using BookWiki.Core;
using BookWiki.Core.Files.FileSystemModels;
using BookWiki.Core.Files.PathModels;
using BookWiki.Core.Utils;
using BookWiki.Presentation.Apple.Extentions;
using BookWiki.Presentation.Apple.Views.Common;
using Keurig.IQ.Core.CrossCutting.Extensions;

namespace BookWiki.Presentation.Apple.Views.Controls
{
    public class FileSystemTreeView : View
    {
        private CollectionView _filePaths;
        private IFileSystemNode _root;
        private readonly Action _enableScheme;
        private readonly Action _disableScheme;

        public event Action<IAbsolutePath> Selected = delegate { };

        public FileSystemTreeView(IFileSystemNode root, Action enableScheme, Action disableScheme)
        {
            _root = root;
            _enableScheme = enableScheme;
            _disableScheme = disableScheme;
            Initialize();
        }

        private void Initialize()
        {
            _filePaths = new CollectionView();
            Add(_filePaths);

            _filePaths.AddRangeWithoutAnimation(new CollectionItem(new FileSystemNodeView(_root, path => Selected(path), _disableScheme, _enableScheme), new HorizontalSeparatorView()));

            Layout = () =>
            {
                _filePaths.ChangeSize(Frame.Width, Frame.Height);
                _filePaths.ChangePosition(0, 0);
            };

            Layout();
        }

        public FileSystemNodeView SelectedContent
        {
            get => _filePaths.Items.FirstOrDefault(x => x.Content.CastTo<ISelectable>().IsSelected)?.Content.CastTo<FileSystemNodeView>();
        }

        public int SelectedIndex => _filePaths.Items.IndexOf(x => x.Content.CastTo<ISelectable>().IsSelected);

        public void TapSelected()
        {
            SelectedContent?.Tap();
        }

        public void Select(int index)
        {
            var item = _filePaths.Items.ElementAtOrDefault(index);

            if (item != null)
            {
                _filePaths.Select(item);
            }
        }
    }
}