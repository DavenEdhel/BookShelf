using System;
using BookMap.Presentation.Apple;
using BookWiki.Core.Files.FileSystemModels;
using BookWiki.Core.Files.PathModels;
using BookWiki.Presentation.Apple.Controllers;
using BookWiki.Presentation.Apple.Extentions;
using BookWiki.Presentation.Apple.Models.HotKeys;
using UIKit;
using Application = BookWiki.Presentation.Apple.Models.Application;

namespace BookWiki.Presentation.Apple.Views.NewNodeDialog
{
    public class TypeSelectDialogView : View
    {
        private readonly IAbsolutePath _rootPath;
        private readonly Action _tearDown;
        private TypeSelectView _typeSelectView;
        private HotKeyScheme _scheme;
        public event Action<IAbsolutePath, IExtension> OnSave = delegate {  };

        public TypeSelectDialogView(IAbsolutePath rootPath, Action tearDown)
        {
            _rootPath = rootPath;
            _tearDown = tearDown;
            Initialize();
        }

        private void Initialize()
        {
            _scheme = new HotKeyScheme(
                new HotKey(Key.Enter, Save),
                new HotKey(Key.Escape, Hide)
            );

            _typeSelectView = new TypeSelectView();
            _typeSelectView.OnCancel += Hide;
            _typeSelectView.OnCreate += TypeSelectViewOnOnCreate;
            Add(_typeSelectView);

            BackgroundColor = UIColor.DarkGray.ColorWithAlpha(0.3f);

            Layout = () =>
            {
                _typeSelectView.SetSizeThatFits();
                _typeSelectView.PositionToCenterInside(this);
            };

            Layout();
        }

        private void Save()
        {
            TypeSelectViewOnOnCreate(_typeSelectView.Name, _typeSelectView.Extension);
        }

        private void TypeSelectViewOnOnCreate(IFileName arg1, IExtension arg2)
        {
            var path = new FolderPath(_rootPath, arg1, arg2);

            OnSave(path, arg2);

            Hide();
        }

        public TypeSelectDialogView Show()
        {
            var rootView = AppDelegate.MainWindow.RootViewController.View;

            Frame = rootView.Frame;
            rootView.Add(this);

            EnableScheme();

            return this;
        }

        public void Hide()
        {
            RemoveFromSuperview();

            DisableScheme();

            _tearDown();
        }

        private void EnableScheme()
        {
            Application.Instance.RegisterSchemeForEditMode(_scheme);
        }

        private void DisableScheme()
        {
            Application.Instance.UnregisterScheme(_scheme);
        }

        public void SelectNovel()
        {
            _typeSelectView.Select(NodeType.Novel);
            _typeSelectView.Focus();
        }

        public void SelectFolder()
        {
            _typeSelectView.Select(NodeType.Directory);
            _typeSelectView.Focus();
        }
    }
}