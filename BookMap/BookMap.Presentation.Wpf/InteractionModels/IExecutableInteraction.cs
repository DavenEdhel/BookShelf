using System.Reactive.Subjects;
using System.Windows.Input;

namespace BookMap.Presentation.Wpf.InteractionModels
{
    public interface IExecutableInteraction : IInteraction, IActive, IBackgroundInteraction
    {
        public class Decorator : IExecutableInteraction
        {
            private readonly IExecutableInteraction _origin;

            public Decorator(IExecutableInteraction origin)
            {
                _origin = origin;
            }

            public virtual void OnKeyDown(KeyEventArgs keyEventArgs)
            {
                _origin.OnKeyDown(keyEventArgs);
            }

            public virtual void OnKeyUp(KeyEventArgs keyEventArgs)
            {
                _origin.OnKeyUp(keyEventArgs);
            }

            public virtual void OnMouseDown(MouseButtonEventArgs mouseButtonEventArgs)
            {
                _origin.OnMouseDown(mouseButtonEventArgs);
            }

            public virtual void OnMouseMove(MouseEventArgs mouseEventArgs)
            {
                _origin.OnMouseMove(mouseEventArgs);
            }

            public virtual void OnMouseUp(MouseButtonEventArgs mouseButtonEventArgs)
            {
                _origin.OnMouseUp(mouseButtonEventArgs);
            }

            public virtual void OnMouseWheel(MouseWheelEventArgs mouseWheelEventArgs)
            {
                _origin.OnMouseWheel(mouseWheelEventArgs);
            }

            public BehaviorSubject<bool> IsActive => _origin.IsActive;

            public bool IsBackground => _origin.IsBackground;
        }
    }
}