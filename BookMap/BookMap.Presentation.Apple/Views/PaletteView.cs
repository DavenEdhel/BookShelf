using System;
using System.Collections.Generic;
using System.Linq;
using BookMap.Presentation.Apple.Extentions;
using BookMap.Presentation.Apple.Services;
using CoreGraphics;
using UIKit;

namespace BookMap.Presentation.Apple.Views
{
    public class PaletteView : UIView
    {
        private readonly MapProvider _mapProvider;

        public PaletteView(MapProvider mapProvider)
        {
            _mapProvider = mapProvider;
            Initialize();
        }

        public BrushTile CurrentBrush => _selectedColorTile;

        public event Action<BrushTile> EditBrush = delegate { };

        public float Scale
        {
            set
            {
                foreach (var brushTile in _brushes)
                {
                    brushTile.Scale = value;
                }
            }
        }

        private readonly List<Brush> _preferred = new List<Brush>();

        private BrushTile _selectedColorTile;

        private readonly BrushTile[] _brushes = new BrushTile[9];

        private void Initialize()
        {
            _mapProvider.MapChanged += MapProviderOnMapChanged;

            BackgroundColor = UIColor.LightGray.ColorWithAlpha(0.5f);

            var bottomOffset = 0;

            var i = 0;

            LoadBrushes();

            foreach (var brush in _preferred)
            {
                var tile = new BrushTile();
                tile.Frame = new CGRect(0, bottomOffset, 75, 75);
                tile.Brush = brush;
                tile.ClipsToBounds = true;
                tile.Clicked += TileOnTouchUpInside;

                Add(tile);

                bottomOffset += 75;

                _brushes[i] = tile;
                i++;
            }

            Select(_brushes[0]);
        }

        private void LoadBrushes()
        {
            _preferred.Clear();
            _preferred.Add(Brush.Eraser);

            _preferred.AddRange(_mapProvider.Settings.Brushes.Select(x => new Brush(x.Size, x.Color.ToUIColor())));
        }

        private void MapProviderOnMapChanged(string obj)
        {
            LoadBrushes();

            for (int i = 0; i < _preferred.Count; i++)
            {
                _brushes[i].Brush = _preferred[i];
            }
        }

        private void TileOnTouchUpInside(object sender, EventArgs eventArgs)
        {
            Select((BrushTile) sender);
        }

        private void Select(BrushTile tile)
        {
            if (ReferenceEquals(_selectedColorTile, tile))
            {
                EditBrush(tile);

                return;
            }

            _selectedColorTile = tile;

            foreach (var brushTile in _brushes)
            {
                brushTile.BackgroundColor = UIColor.Clear;
            }

            _selectedColorTile.BackgroundColor = UIColor.DarkGray;
        }

        public void SaveBrushes()
        {
            _mapProvider.ChangeSettings(settings =>
            {
                settings.Brushes = _brushes.Where(x => x.IsEraser == false).Select(x => x.Brush.Size.ToBrushInfoWithColor(x.Brush.Color)).ToArray();
            });
        }
    }
}