using System;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BookMap.Presentation.Apple.Models;
using BookMap.Presentation.Wpf.Core;
using BookMap.Presentation.Wpf.MapModels.DrawModels;

namespace BookMap.Presentation.Wpf.InteractionModels
{
    public interface ICursor
    {
        void Show();

        void Hide();
    }

    public class LabeledCursor : ICursor
    {
        private readonly CurrentBrush _currentBrush;
        private readonly CoordinateSystem _coordinates;
        private readonly Image _cursor;
        private readonly TextBlock _label;
        private Point _lastPosition = new Point(0, 0);
        private IBrush _lastBrush = new EraserBrush();
        private readonly IDrawingShape _shape;

        public LabeledCursor(Canvas container, CurrentBrush currentBrush, CoordinateSystem coordinates, IDrawingShape shape = null)
        {
            _shape = shape ?? new Circle();
            _currentBrush = currentBrush;
            _coordinates = coordinates;
            var cursorSize = 1536 * 10 / 2560;

            _cursor = new Image();
            _cursor.Width = cursorSize;
            _cursor.Height = cursorSize;
            Canvas.SetLeft(_cursor, 100);
            Canvas.SetTop(_cursor, 100);
            container.Children.Add(_cursor);

            _label = new TextBlock();
            _label.Text = "labels";
            Canvas.SetLeft(_label, 100);
            Canvas.SetTop(_label, 100);
            container.Children.Add(_label);
            
            Draw();

            currentBrush
                .Subscribe(
                    brush =>
                    {
                        PositionAndResizeAndColorize(_lastPosition);
                    }
                );

            _coordinates.LevelScaleChanged += CoordinatesOnLevelScaleChanged;
        }

        private void CoordinatesOnLevelScaleChanged(double obj)
        {
            _lastBrush = new EraserBrush();

            PositionAndResizeAndColorize(_lastPosition);
        }

        public string Title
        {
            get => _label.Text;
            set
            {
                _label.Text = value;
            }
        }

        public double CursorSize => (1536 * (_currentBrush.SizeInPixels) / 2560 * _coordinates.ScaleBetweenLevels);

        public void PositionAndResizeAndColorize(Point current)
        {
            _lastPosition = current;

            var cursorSize = CursorSize;

            _cursor.Width = cursorSize;
            _cursor.Height = cursorSize;

            var cx = current.X - cursorSize / 2;
            Canvas.SetLeft(_cursor, cx);
            var cy = current.Y - cursorSize / 2;
            Canvas.SetTop(_cursor, cy);

            Canvas.SetLeft(_label, cx + cursorSize);
            Canvas.SetTop(_label, cy);

            if (_lastBrush.EqualsTo(_currentBrush) == false)
            {
                _lastBrush = _currentBrush.CastTo<IBrush>().Snapshot();

                Draw();
            }
        }

        private void Draw()
        {
            var cursorSizeInPixels = _currentBrush.SizeInPixels;

            var cursorSource = new WriteableBitmap(cursorSizeInPixels, cursorSizeInPixels, 96, 96, PixelFormats.Bgra32, null);

            cursorSource.Draw(new Point(0, 0), _currentBrush, _shape);

            _cursor.Source = cursorSource;
        }

        public void Show()
        {
            _cursor.Visibility = Visibility.Visible;
            _label.Visibility = Visibility.Visible;
        }

        public void Hide()
        {
            _cursor.Visibility = Visibility.Hidden;
            _label.Visibility = Visibility.Hidden;
        }
    }
}