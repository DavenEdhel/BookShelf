using BookMap.Presentation.Apple;
using BookWiki.Core;
using BookWiki.Core.Files.FileModels;
using BookWiki.Core.Files.FileSystemModels;
using BookWiki.Core.Files.PathModels;
using BookWiki.Presentation.Apple.Controllers;
using BookWiki.Presentation.Apple.Extentions;
using BookWiki.Presentation.Apple.Models.HotKeys;
using BookWiki.Presentation.Apple.Views.Main;
using BookWiki.Presentation.Apple.Views.NewNodeDialog;
using Application = BookWiki.Presentation.Apple.Models.Application;

namespace BookWiki.Presentation.Apple.Views.Controls
{
    public class FileSystemView : View, IContentView
    {
        private readonly IFileSystemNode _node;
        private readonly TabCollectionView _tabCollectionView;
        private readonly ILibrary _library;

        private HotKeyScheme _scheme;
        private FileSystemTreeView _treeView;
        private TypeSelectDialogView _dialog;

        public FileSystemView(IFileSystemNode node, TabCollectionView tabCollectionView, ILibrary library)
        {
            _node = node;
            _tabCollectionView = tabCollectionView;
            _library = library;
            Initialize();
        }

        private void Initialize()
        {
            _scheme = new HotKeyScheme(
                new HotKey(new Key("n"), CreateNovel),
                new HotKey(new Key("f"), CreateFolder),
                new HotKey(Key.ArrowUp, Up),
                new HotKey(Key.ArrowDown, Down),
                new HotKey(Key.Enter, Tap));

            _treeView = new FileSystemTreeView(_node, Show, Hide);
            _treeView.Selected += TreeViewOnSelected;

            Add(_treeView);

            Layout = () =>
            {
                _treeView.ChangeSize(Frame.Width, Frame.Height);
                _treeView.ChangePosition(0, 0);
            };

            Layout();
        }

        private void Tap()
        {
            _treeView.TapSelected();
        }

        private void Down()
        {
            if (_treeView.SelectedContent == null)
            {
                _treeView.Select(0);
                return;
            }
            
            _treeView.Select(_treeView.SelectedIndex + 1);
        }

        private void Up()
        {
            if (_treeView.SelectedContent == null)
            {
                _treeView.Select(0);
                return;
            }

            _treeView.Select(_treeView.SelectedIndex - 1);
        }

        private void CreateFolder()
        {
            _treeView.SelectedContent?.ShowDialogForFolder();
        }

        private void CreateNovel()
        {
            _treeView.SelectedContent?.ShowDialogForNovel();
        }

        private void TreeViewOnSelected(IPath novelPath)
        {
            if (novelPath.Extension.Type == NodeType.Directory)
            {
                return;
            }

            _tabCollectionView.SelectTab(_library.Load(novelPath));
        }

        public void Hide()
        {
            Application.Instance.UnregisterScheme(_scheme);
        }

        public void Show()
        {
            Application.Instance.RegisterSchemeForViewMode(_scheme);
        }
    }
}