using System;
using BookWiki.Core.Files.FileSystemModels;
using BookWiki.Presentation.Apple.Extentions;
using CoreGraphics;
using Foundation;
using UIKit;

namespace BookWiki.Presentation.Apple.Views.NewNodeDialog
{
    public class TypeView : View
    {
        private readonly NodeType _nodeType;

        public NodeType Type => _nodeType;

        public event Action<TypeView> OnSelected = delegate {  };

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected == value)
                {
                    return;
                }

                _isSelected = value;

                if (IsSelected)
                {
                    Layer.BorderWidth = 1;
                    Layer.BorderColor = UIColor.DarkGray.CGColor;

                    OnSelected(this);
                }
                else
                {
                    Layer.BorderWidth = 0;
                }
            }
        }

        private UIImageView _image;
        private bool _isSelected;

        public TypeView(NodeType nodeType)
        {
            _nodeType = nodeType;
            Initialize();
        }

        private void Initialize()
        {
            _image = new UIImageView(GetImage());
            Add(_image);

            Layout = () =>
            {
                _image.SetSizeThatFits();
                _image.ChangePosition(0, 0);
            };

            Layout();

            UIImage GetImage()
            {
                switch (_nodeType)
                {
                    case NodeType.Novel:
                        return UIImage.FromBundle("poem");
                    default:
                        return UIImage.FromBundle("folder");
                }
            }
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(touches, evt);

            IsSelected = true;
        }

        public override CGSize SizeThatFits(CGSize size)
        {
            Layout();

            return _image.Frame.Size;
        }
    }
}