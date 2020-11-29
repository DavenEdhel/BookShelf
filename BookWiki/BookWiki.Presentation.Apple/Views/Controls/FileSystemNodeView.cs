using System;
using System.Collections.Generic;
using System.Linq;
using BookMap.Presentation.Apple;
using BookWiki.Core;
using BookWiki.Core.Files.FileSystemModels;
using BookWiki.Core.Files.PathModels;
using BookWiki.Presentation.Apple.Extentions;
using BookWiki.Presentation.Apple.Models;
using BookWiki.Presentation.Apple.Views.Common;
using BookWiki.Presentation.Apple.Views.Main;
using BookWiki.Presentation.Apple.Views.NewNodeDialog;
using CoreGraphics;
using Keurig.IQ.Core.CrossCutting.Extensions;
using UIKit;

namespace BookWiki.Presentation.Apple.Views.Controls
{
    public class FileSystemNodeView : Control, IComparable, ICollectionBindable, ISelectable
    {
        private readonly TabCollectionView _tabs;
        private readonly IFileSystemNode _fileSystemNode;
        private readonly Action _disableScheme;
        private readonly Action _enableScheme;
        private CollectionView _collectionView;
        private CollectionItem _collectionItem;
        private UILabel _label;
        private int _margin = 10;

        private bool _isClosed = false;

        private readonly nfloat _marginLeft;
        private UIButton _expandButton;
        private UIButton _addButton;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;

                    BackgroundColor = value ? UIColor.LightGray : UIColor.White;
                }
            }
        }

        public Action<IAbsolutePath> OnSelected { get; set; }

        private readonly List<CollectionItem> _collectionItems = new List<CollectionItem>();
        private TypeSelectDialogView _dialog;
        private bool _isSelected;

        public IFileSystemNode Model => _fileSystemNode;

        public FileSystemNodeView(TabCollectionView tabs, IFileSystemNode fileSystemNode, Action<IAbsolutePath> onSelected, Action disableScheme, Action enableScheme)
        {
            _tabs = tabs;
            _fileSystemNode = fileSystemNode;
            _disableScheme = disableScheme;
            _enableScheme = enableScheme;

            _marginLeft = 20 * fileSystemNode.Level;

            OnSelected = onSelected;

            Initialize();
        }

        private void Initialize()
        {
            ClipsToBounds = true;

            _expandButton = new UIButton(UIButtonType.RoundedRect);
            _expandButton.SetImage(UIImage.FromBundle("Back").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
            _expandButton.Layer.AffineTransform = CGAffineTransform.MakeRotation(90 * NMath.PI / 180);
            _expandButton.TintColor = UIColor.FromRGBA(red: 0.18f, green: 0.16f, blue: 0.15f, alpha: 1);
            _expandButton.Hidden = _fileSystemNode.IsContentFolder;
            _expandButton.TouchUpInside += (sender, args) =>
            {
                Toggle();
            };
            Add(_expandButton);

            _addButton = new UIButton(UIButtonType.RoundedRect);
            _addButton.SetImage(UIImage.FromBundle("add"), UIControlState.Normal);
            _addButton.TouchUpInside += AddButtonOnTouchUpInside;
            _addButton.Hidden = _fileSystemNode.IsContentFolder;
            Add(_addButton);

            _label = new UILabel();
            _label.Lines = 0;
            _label.Text = _fileSystemNode.Path.Name.PlainText;
            Add(_label);

            Layout = () =>
            {
                _expandButton.SetSizeThatFits();
                var margin = (Frame.Height - _expandButton.Frame.Height) / 2;
                _expandButton.ChangePosition(margin + _marginLeft, margin);

                _label.SetSizeThatFits(Frame.Width - _marginLeft - Frame.Height * 2 - _expandButton.Frame.Width - 5);
                _label.ChangeY(_margin);
                _label.ChangeX(_expandButton.Frame.Right + 10);

                _addButton.ChangeHeight(Frame.Height);
                _addButton.ChangeWidth(Frame.Height * 2);
                _addButton.PositionToRightAndCenterInside(this, _margin);
            };

            Layout();

            TouchUpInside += OnTouchUpInside;

            Toggle(toClose: _fileSystemNode.SubitemsArePresentInNavigationPanel(new UserFolderPath(), _tabs.OpenedCustomTabs.Select(x => x.Data as IContent).Where(x => x != null)) == false);
        }

        private void AddButtonOnTouchUpInside(object sender, EventArgs e)
        {
            ShowDialog();
        }

        public void ShowDialog()
        {
            _disableScheme();

            _dialog = new TypeSelectDialogView(_fileSystemNode.Path, _enableScheme).Show();

            _dialog.OnSave += DialogOnOnSave;
        }

        public void ShowDialogForNovel()
        {
            _disableScheme();

            _dialog = new TypeSelectDialogView(_fileSystemNode.Path, _enableScheme).Show();

            _dialog.SelectNovel();

            _dialog.OnSave += DialogOnOnSave;
        }

        public void ShowDialogForFolder()
        {
            _disableScheme();

            _dialog = new TypeSelectDialogView(_fileSystemNode.Path, _enableScheme).Show();

            _dialog.SelectFolder();

            _dialog.OnSave += DialogOnOnSave;
        }


        private void DialogOnOnSave(IAbsolutePath path, IExtension extension)
        {
            var fileSystemNode = new FileSystemNode(_fileSystemNode, path);
            fileSystemNode.SaveUnder(_fileSystemNode);

            var item = new CollectionItem(new FileSystemNodeView(_tabs, fileSystemNode, OnSelected, _disableScheme, _enableScheme), new HorizontalSeparatorView());
            _collectionView.Add(item, animated: true);
            _collectionItems.Add(item);
        }

        private void Toggle(bool? toClose = null)
        {
            if (_fileSystemNode.IsContentFolder)
            {
                return;
            }

            if (toClose.HasValue)
            {
                _isClosed = toClose.Value;
            }
            else
            {
                _isClosed = !_isClosed;
            }

            if (_isClosed)
            {
                foreach (var collectionItem in _collectionItems)
                {
                    collectionItem.Content.CastTo<FileSystemNodeView>().Toggle(toClose: true);

                    _collectionView?.Remove(collectionItem, animated: true);
                }

                _collectionItems.Clear();
            }
            else
            {
                foreach (var fileSystemNode in _fileSystemNode.InnerNodes)
                {
                    var item = new CollectionItem(new FileSystemNodeView(_tabs, fileSystemNode, OnSelected, _disableScheme, _enableScheme), new HorizontalSeparatorView());
                    _collectionItems.Add(item);

                    _collectionView?.Add(item, animated: true);
                }
            }

            Animate(
                0.2f,
                () =>
                {
                    var angle = _isClosed ? 270 * NMath.PI / 180 : 90 * NMath.PI / 180;

                    _expandButton.Layer.AffineTransform = CGAffineTransform.MakeRotation(angle);
                });
        }

        private void OnTouchUpInside(object sender, EventArgs e)
        {
            Tap();
        }

        public override CGSize SizeThatFits(CGSize size)
        {
            var x = _label.SizeThatFits(new CGSize(size.Width - _margin - _marginLeft - 35, nfloat.MaxValue));

            return new CGSize(size.Width, x.Height + _margin * 2);
        }

        public int CompareTo(object obj)
        {
            if (obj is FileSystemNodeView anotherNode)
            {
                return (int)new FileSystemNodeEquality(_fileSystemNode, anotherNode._fileSystemNode).EqualityState;
            }

            return -1;
        }

        public void BindWithCollection(CollectionView collectionView, CollectionItem collectionItem)
        {
            _collectionView = collectionView;
            _collectionItem = collectionItem;

            _collectionView.AddRangeWithoutAnimation(_collectionItems.ToArray());
        }

        public void Tap()
        {
            Toggle();

            _collectionView?.Select(_collectionItem);

            OnSelected?.Invoke(_fileSystemNode.Path);
        }
    }
}