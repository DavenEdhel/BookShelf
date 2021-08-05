using System;
using CoreGraphics;
using UIKit;

namespace BookMap.Presentation.Apple.Views
{
    public class BrushTile : UIControl
    {
        private UIControl _selectedColorTile;
        private float _scale = 1;
        private float _size = 5f;

        public event EventHandler Clicked = delegate { };

        public float Scale
        {
            get { return _scale; }
            set
            {
                _scale = value; 
                Layout();
            }
        }

        public float Size
        {
            get { return _size; }
            set
            {
                _size = value;
                Layout();
            }
        }

        public bool IsEraser { get; private set; }

        public UIColor Color
        {
            get => _selectedColorTile.BackgroundColor;
            set => _selectedColorTile.BackgroundColor = value;
        }

        private void Layout()
        {
            var w = Size * Scale;

            var off = (75 - w) / 2;

            _selectedColorTile.Frame = new CGRect(off, off, w, w);
            _selectedColorTile.Layer.CornerRadius = w / 2;
        }

        public BrushTile()
        {
            Initialize();
        }

        private void Initialize()
        {
            _selectedColorTile = new UIControl();
            _selectedColorTile.TouchUpInside += SelectedColorTileOnTouchUpInside;
            _selectedColorTile.BackgroundColor = UIColor.Red;
            Add(_selectedColorTile);

            TouchUpInside += OnTouchUpInside;

            Layout();
        }

        private void OnTouchUpInside(object sender, EventArgs eventArgs)
        {
            Clicked(this, eventArgs);
        }

        private void SelectedColorTileOnTouchUpInside(object sender, EventArgs eventArgs)
        {
            Clicked(this, eventArgs);
        }

        public Brush Brush
        {
            get
            {
                return new Brush(Size, Color)
                {
                    IsEraser = IsEraser
                };
            }
            set
            {
                Size = value.Size;
                Color = value.Color;
                IsEraser = value.IsEraser;
            }
        }
    }
}