using System;
using System.Linq;
using BookWiki.Core.Files.FileSystemModels;
using BookWiki.Core.Files.PathModels;
using BookWiki.Presentation.Apple.Extentions;
using CoreGraphics;
using UIKit;

namespace BookWiki.Presentation.Apple.Views.NewNodeDialog
{
    public class TypeSelectView : View
    {
        private int _margin = 10;
        private UITextField _editText;
        private UIButton _save;
        private UIButton _cancel;

        public event Action OnCancel = delegate {  };
        public event Action<IFileName, IExtension> OnCreate = delegate {  };

        private TypeView[] _types = new TypeView[2];

        public IFileName Name => new FileName(_editText.Text);

        public IExtension Extension => new Extension(_types.First(x => x.IsSelected).Type);

        public TypeSelectView()
        {
            Initialize();
        }

        private void Initialize()
        {
            BackgroundColor = UIColor.White;

            _types[0] = new TypeView(NodeType.Novel);
            _types[1] = new TypeView(NodeType.Directory);
            _editText = new UITextField();
            _editText.TextAlignment = UITextAlignment.Center;

            foreach (var typeView in _types)
            {
                typeView.OnSelected += SelectType;
            }

            Add(_types[0]);
            Add(_types[1]);
            Add(_editText);

            _save = new UIButton(UIButtonType.RoundedRect);
            _save.SetTitleColor(UIColor.Black, UIControlState.Normal);
            _save.SetTitle("ОК", UIControlState.Normal);
            _save.TouchUpInside += SaveOnTouchUpInside;
            Add(_save);

            _cancel = new UIButton(UIButtonType.RoundedRect);
            _cancel.SetTitleColor(UIColor.Black, UIControlState.Normal);
            _cancel.SetTitle("Отмена", UIControlState.Normal);
            _cancel.TouchUpInside += CancelOnTouchUpInside;
            Add(_cancel);

            Layout = () =>
            {
                _types[0].SetSizeThatFits();
                _types[1].SetSizeThatFits();

                _types[0].ChangePosition(_margin, _margin);
                _types[1].ChangePosition(_types[0].Frame.Right + 5, _margin);

                _editText.SetSizeThatFits();
                _editText.PositionUnder(_types[0], _margin);
                _editText.ChangeWidth(_types[1].Frame.Right - _margin);
                _editText.ChangeX(_margin);

                _save.SetSizeThatFits();
                _cancel.SetSizeThatFits();

                _save.ChangeHeight(_editText.Frame.Height);
                _cancel.ChangeHeight(_editText.Frame.Height);

                _save.PositionUnder(_editText, _margin);
                _cancel.PositionUnder(_editText, _margin);

                var width = _types[1].Frame.Right + 5;
                var center = width / 2;
                var buttonCenter = center / 2;
                _save.ChangeX(_margin + buttonCenter - _save.Frame.Width/2);
                _cancel.ChangeX(center + _margin + buttonCenter - _cancel.Frame.Width / 2);
            };

            Layout();
        }

        private void SelectType(TypeView self)
        {
            foreach (var typeView in _types)
            {
                typeView.IsSelected = typeView.Type == self.Type;
            }
        }

        private void CancelOnTouchUpInside(object sender, EventArgs e)
        {
            OnCancel();
        }

        private void SaveOnTouchUpInside(object sender, EventArgs e)
        {
            OnCreate(Name, Extension);
        }

        public override CGSize SizeThatFits(CGSize size)
        {
            Layout();

            return new CGSize(_cancel.Frame.Right + _margin, _cancel.Frame.Bottom + _margin);
        }

        public void Select(NodeType type)
        {
            SelectType(_types.First(x => x.Type == type));
        }

        public void Focus()
        {
            _editText.BecomeFirstResponder();
        }
    }
}