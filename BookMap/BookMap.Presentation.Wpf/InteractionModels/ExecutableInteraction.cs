using System.Reactive.Subjects;
using System.Windows.Input;

namespace BookMap.Presentation.Wpf.InteractionModels
{
    public abstract class ExecutableInteraction : IExecutableInteraction
    {
        public virtual bool CanUseSimultaneously => true;

        public BehaviorSubject<bool> Captured { get; } = new(false);

        public virtual void OnKeyDown(KeyEventArgs keyEventArgs)
        {
        }

        public virtual void OnKeyUp(KeyEventArgs keyEventArgs){}

        public virtual void OnMouseDown(MouseButtonEventArgs mouseButtonEventArgs)
        {

        }

        public virtual void OnMouseMove(MouseEventArgs mouseEventArgs)
        {
        }

        public virtual void OnMouseUp(MouseButtonEventArgs mouseButtonEventArgs){}

        public virtual void OnMouseWheel(MouseWheelEventArgs mouseWheelEventArgs){}

        public BehaviorSubject<bool> IsActive { get; } = new(false);

        public virtual bool IsBackground => false;
    }
}