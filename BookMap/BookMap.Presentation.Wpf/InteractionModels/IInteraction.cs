using System.Windows.Input;

namespace BookMap.Presentation.Wpf.InteractionModels
{
    public interface IInteraction
    {
        void OnKeyDown(KeyEventArgs keyEventArgs);

        void OnKeyUp(KeyEventArgs keyEventArgs);

        void OnMouseDown(MouseButtonEventArgs mouseButtonEventArgs);

        void OnMouseMove(MouseEventArgs mouseEventArgs);

        void OnMouseUp(MouseButtonEventArgs mouseButtonEventArgs);

        void OnMouseWheel(MouseWheelEventArgs mouseWheelEventArgs);
    }
}