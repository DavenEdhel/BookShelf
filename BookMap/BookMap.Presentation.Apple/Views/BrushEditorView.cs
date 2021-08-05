using System;
using System.Collections.Generic;
using System.Linq;
using CoreGraphics;
using UIKit;

namespace BookMap.Presentation.Apple.Views
{
    public class BrushEditorView : UIView
    {
        private readonly MapView _mapView;
        private readonly PaletteView _paletteView;
        private BrushTile _tile;
        private Brush _initialBrush = new Brush(12, UIColor.Cyan);
        private UIButton _close;
        private UIButton _apply;
        private BrushTile _currentTile;
        private SizeBarView _sizeSlider;

        private ColorPartBarView _r, _g, _b, _h, _s, _v;
        private UIView _colorView;
        private ColorPartBarView _part;
        private GradientBackgroundStrategy _red;

        private IEnumerable<UIView> Bars
        {
            get
            {
                foreach (var colorPartBarView in Colors)
                {
                    yield return colorPartBarView;
                }

                yield return _sizeSlider;
            }
        }

        private IEnumerable<ColorPartBarView> Colors
        {
            get
            {
                yield return _r;
                yield return _g;
                yield return _b;
                yield return _h;
                yield return _s;
                yield return _v;
            }
        }

        private UIColor _colorBeforeSelect;
        private UIButton _cancelSelect;

        public void SetColorFromSelector(UIColor color)
        {
            if (_colorBeforeSelect == null)
            {
                _colorBeforeSelect = Tile.Color;
            }

            Add(_cancelSelect);

            Tile.Color = color;

            SetColorEqualsToTile();
        }

        public BrushTile Tile
        {
            get { return _tile; }
            set
            {
                _tile = value;
                _initialBrush = value.Brush;
                _currentTile.Brush = _initialBrush;

                _sizeSlider.CurrentSize = _initialBrush.Size;

                SetColorEqualsToTile();
            }
        }

        private void SetColorEqualsToTile()
        {
            _currentTile.Color = Tile.Color;
            _colorView.BackgroundColor = Tile.Color;

            foreach (var colorPartBarView in Colors)
            {
                colorPartBarView.NotifyColorChanged(Tile.Color);
            }
        }

        public BrushEditorView(MapView mapView, PaletteView paletteView)
        {
            _mapView = mapView;
            _paletteView = paletteView;
            Initialize();
        }

        private void Initialize()
        {
            BackgroundColor = UIColor.White;

            _close = new UIButton(UIButtonType.RoundedRect);
            _close.SetTitleColor(UIColor.Black, UIControlState.Normal);
            _close.SetTitle("Close", UIControlState.Normal);
            _close.TouchUpInside += CloseOnTouchUpInside;
            Add(_close);

            _apply = new UIButton(UIButtonType.RoundedRect);
            _apply.SetTitleColor(UIColor.Black, UIControlState.Normal);
            _apply.SetTitle("Ok", UIControlState.Normal);
            _apply.TouchUpInside += ApplyOnTouchUpInside;
            Add(_apply);

            _cancelSelect = new UIButton(UIButtonType.RoundedRect);
            _cancelSelect.SetTitleColor(UIColor.Black, UIControlState.Normal);
            _cancelSelect.SetTitle("Cancel", UIControlState.Normal);
            _cancelSelect.TouchUpInside += CancelSelect;
            
            _currentTile = new BrushTile();
            Add(_currentTile);

            _colorView = new UIView();
            Add(_colorView);

            _sizeSlider = new SizeBarView();
            _sizeSlider.MinValue = 1;
            _sizeSlider.MaxValue = 40;
            _sizeSlider.SizeChanged += SizeSliderOnValueChanged;
            Add(_sizeSlider);

            _r = ColorPartBarView.Gradient(ColorPart.R, _initialBrush.Color);
            _r.ColorChanged += PartOnColorChanged;
            Add(_r);

            _g = ColorPartBarView.Gradient(ColorPart.G, _initialBrush.Color);
            _g.ColorChanged += PartOnColorChanged;
            Add(_g);

            _b = ColorPartBarView.Gradient(ColorPart.B, _initialBrush.Color);
            _b.ColorChanged += PartOnColorChanged;
            Add(_b);

            _h = ColorPartBarView.Hue(_initialBrush.Color);
            _h.ColorChanged += PartOnColorChanged;
            Add(_h);

            _s = ColorPartBarView.Gradient(ColorPart.S, _initialBrush.Color);
            _s.ColorChanged += PartOnColorChanged;
            Add(_s);

            _v = ColorPartBarView.Gradient(ColorPart.V, _initialBrush.Color);
            _v.ColorChanged += PartOnColorChanged;
            Add(_v);

        }

        private void CancelSelect(object sender, EventArgs e)
        {
            Tile.Color = _colorBeforeSelect;
            SetColorEqualsToTile();
            _cancelSelect.RemoveFromSuperview();
        }

        private void PartOnColorChanged(UIColor uiColor, ColorPart colorPart)
        {
            foreach (var colorPartBarView in Colors)
            {
                if (colorPartBarView.LockedPart != colorPart)
                {
                    colorPartBarView.NotifyColorChanged(uiColor);
                }
            }

            SetColor(uiColor);

            Console.WriteLine(uiColor.ToString());
        }

        private void SetColor(UIColor color)
        {
            _tile.Color = color;
            _currentTile.Color = color;
            _colorView.BackgroundColor = color;
        }


        private void SizeSliderOnValueChanged(float value)
        {
            _tile.Size = value;
            _currentTile.Size = value;
        }

        private void ApplyOnTouchUpInside(object sender, EventArgs eventArgs)
        {
            _mapView.ColorEditMode = false;

            _paletteView.SaveBrushes();

            RemoveFromSuperview();
        }

        private void CloseOnTouchUpInside(object sender, EventArgs eventArgs)
        {
            _tile.Brush = _initialBrush;
            _mapView.ColorEditMode = false;

            RemoveFromSuperview();
        }

        public override void LayoutSubviews()
        {
            var closeFrame = _close.SizeThatFits(Frame.Size);
            _close.Frame = new CGRect(new CGPoint(Frame.Width - closeFrame.Width - 5, 0), closeFrame);

            _currentTile.Frame = new CGRect(5, _close.Frame.Bottom, 75, 75);

            _colorView.Frame = new CGRect(_currentTile.Frame.Right, _close.Frame.Bottom, 75, 75);

            var cancelSelectSize = _cancelSelect.SizeThatFits(new CGSize(float.MaxValue, float.MaxValue));
            _cancelSelect.Frame = new CGRect(_colorView.Frame.Right + 5, _colorView.Frame.Height/2f, cancelSelectSize.Width, cancelSelectSize.Height);

            var bottom = _colorView.Frame.Bottom + 10;

            var left = 5.0f;
            var width = (Frame.Width - 10f) / Bars.Count();

            foreach (var uiSlider in Bars)
            {
                uiSlider.Frame = new CGRect(left, bottom + 10, width - 10, 190);

                left = (float)uiSlider.Frame.Right + 10f;
            }

            var applyFrame = _apply.SizeThatFits(Frame.Size);
            _apply.Frame = new CGRect(new CGPoint(0, Frame.Height - applyFrame.Height), applyFrame);

            base.LayoutSubviews();
        }
    }
}