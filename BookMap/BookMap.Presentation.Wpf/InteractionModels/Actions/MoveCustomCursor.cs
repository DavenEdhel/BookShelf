using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BookMap.Presentation.Wpf.InteractionModels
{
    public class MoveCustomCursor : ExecutableInteraction
    {
        private readonly Canvas _container;

        private readonly LabeledCursor _cursor;
        private Point _current;

        public MoveCustomCursor(
            Canvas container,
            LabeledCursor cursor
        )
        {
            _container = container;
            _cursor = cursor;

            Captured.OnNext(true);
        }

        public override bool IsBackground => true;

        public override bool CanUseSimultaneously => true;

        public override void OnMouseMove(MouseEventArgs e)
        {
            _current = e.GetPosition(_container);

            PositionCursor();
        }

        private void PositionCursor()
        {
            _cursor.PositionAndResizeAndColorize(_current);
        }
    }
}