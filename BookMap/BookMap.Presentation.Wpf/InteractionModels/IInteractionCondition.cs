using System.Reactive.Subjects;
using System.Windows.Input;

namespace BookMap.Presentation.Wpf.InteractionModels
{
    public interface IInteractionCondition : IInteraction, ICapturable, IInteractionConfig
    {
        public class Decorator : IInteractionCondition
        {
            private readonly IInteractionCondition _origin;

            public Decorator(IInteractionCondition origin)
            {
                _origin = origin;
            }

            public virtual bool CanUseSimultaneously => _origin.CanUseSimultaneously;

            public virtual bool IsExclusive => _origin.IsExclusive;

            public virtual BehaviorSubject<bool> Captured => _origin.Captured;

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
        }
    }
}