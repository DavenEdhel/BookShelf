using System.Windows.Input;
using BookMap.Presentation.Wpf.MapModels;

namespace BookMap.Presentation.Wpf.InteractionModels
{
    public class Draw : ExecutableInteraction
    {
        private readonly MapLayer _layer;
        private readonly IBrush _brush;

        public Draw(
            MapLayer layer,
            IBrush brush)
        {
            _layer = layer;
            _brush = brush;
        }

        private bool _moveInProgress;

        public override bool CanUseSimultaneously => false;

        public override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                _layer.Draw(e, _brush);

                _moveInProgress = true;
            }
        }

        public override void OnMouseMove(MouseEventArgs e)
        {
            if (_moveInProgress)
            {
                _layer.Draw(e, _brush);
            }
        }

        public override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (_moveInProgress)
            {
                _moveInProgress = false;

                _layer.Save();
            }
        }
    }
}