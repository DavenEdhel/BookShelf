using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using BookMap.Presentation.Wpf.MapModels;

namespace BookMap.Presentation.Wpf.InteractionModels
{
    public class RenderText : ExecutableInteraction
    {
        private readonly Canvas _canvas;
        private readonly MapLayer _layer;
        private readonly IBrush _brush;
        private readonly Locked _renderTextInteractionLock;
        private readonly LabeledCursor _cursor;
        private readonly Interactions _engine;

        public RenderText(Canvas canvas,
            MapLayer layer,
            IBrush brush,
            Locked renderTextInteractionLock,
            LabeledCursor cursor,
            Interactions engine
        )
        {
            _canvas = canvas;
            _layer = layer;
            _brush = brush;
            _renderTextInteractionLock = renderTextInteractionLock;
            _cursor = cursor;
            _engine = engine;
        }

        private bool _moveInProgress;
        private TextBox _textToRender;
        private Point _absolute;

        public override bool CanUseSimultaneously => false;

        public override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                _moveInProgress = true;
            }
        }

        public override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (_textToRender != null)
            {
                return;
            }

            if (_moveInProgress)
            {
                _moveInProgress = false;

                _textToRender = new TextBox();
                _textToRender.Foreground = new SolidColorBrush(
                    new MediaColorFromArgbColor(
                        new RgbaColorFromBgra(
                            _brush.Color.Bgra
                        )
                    ).Color
                );
                _textToRender.FontSize = _cursor.CursorSize * (72.0 / 96.0);

                _engine.RegisterForInteraction(_textToRender);

                _absolute = e.GetPosition(_canvas);

                _layer.SnapshotPositionsForMaps(e);

                Canvas.SetLeft(_textToRender, _absolute.X);
                Canvas.SetTop(_textToRender, _absolute.Y - _cursor.CursorSize / 2);

                _canvas.Children.Add(_textToRender);

                _textToRender.PreviewKeyDown += TextToRenderOnPreviewKeyDown;

                _textToRender.Focus();

                _renderTextInteractionLock.Lock();
            }
        }

        private void TextToRenderOnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (_textToRender == null)
            {
                return;
            }

            if (e.Key == Key.Enter)
            {
                _layer.Draw(_absolute, _textToRender.Text, _brush, _textToRender);

                _layer.Save();

                _textToRender.PreviewKeyDown -= TextToRenderOnPreviewKeyDown;

                _canvas.Children.Remove(_textToRender);

                _engine.UnregisterFromInteraction(_textToRender);

                _textToRender = null;

                _renderTextInteractionLock.Unlock();
            }

            if (e.Key == Key.Escape)
            {
                _textToRender.PreviewKeyDown -= TextToRenderOnPreviewKeyDown;

                _canvas.Children.Remove(_textToRender);

                _engine.UnregisterFromInteraction(_textToRender);

                _textToRender = null;

                _renderTextInteractionLock.Unlock();
            }
        }
    }
}