using System;
using System.Linq;
using System.Windows.Input;

namespace BookMap.Presentation.Wpf.InteractionModels
{
    public class WhenKeyHold : ExecutableInteraction, IInteractionCondition
    {
        private readonly Key _activationKey;

        public WhenKeyHold(
            Key activationKey)
        {
            _activationKey = activationKey;
        }

        public override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == _activationKey)
            {
                Captured.OnNext(true);
            }
        }

        public override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            if (e.Key == _activationKey)
            {
                Captured.OnNext(false);
            }
        }
    }

    public class Or : ExecutableInteraction, IInteractionCondition
    {
        private readonly IInteractionCondition[] _conditions;

        public Or(
            params IInteractionCondition[] conditions)
        {
            _conditions = conditions;

            foreach (var interactionCondition in _conditions)
            {
                interactionCondition.Captured.Subscribe(
                    captured =>
                    {
                        if (_conditions.All(x => x.Captured.Value))
                        {
                            Captured.OnNext(true);
                        }
                        else
                        {
                            Captured.OnNext(false);
                        }
                    }
                );
            }
        }

        public override void OnKeyUp(KeyEventArgs keyEventArgs)
        {
            base.OnKeyUp(keyEventArgs);

            foreach (var interactionCondition in _conditions)
            {
                interactionCondition.OnKeyUp(keyEventArgs);
            }
        }

        public override void OnKeyDown(KeyEventArgs keyEventArgs)
        {
            base.OnKeyDown(keyEventArgs);

            foreach (var interactionCondition in _conditions)
            {
                interactionCondition.OnKeyDown(keyEventArgs);
            }
        }

        public override void OnMouseDown(MouseButtonEventArgs mouseButtonEventArgs)
        {
            base.OnMouseDown(mouseButtonEventArgs);

            foreach (var interactionCondition in _conditions)
            {
                interactionCondition.OnMouseDown(mouseButtonEventArgs);
            }
        }

        public override void OnMouseMove(MouseEventArgs mouseEventArgs)
        {
            base.OnMouseMove(mouseEventArgs);

            foreach (var interactionCondition in _conditions)
            {
                interactionCondition.OnMouseMove(mouseEventArgs);
            }
        }

        public override void OnMouseUp(MouseButtonEventArgs mouseButtonEventArgs)
        {
            base.OnMouseUp(mouseButtonEventArgs);

            foreach (var interactionCondition in _conditions)
            {
                interactionCondition.OnMouseUp(mouseButtonEventArgs);
            }
        }

        public override void OnMouseWheel(MouseWheelEventArgs mouseWheelEventArgs)
        {
            base.OnMouseWheel(mouseWheelEventArgs);

            foreach (var interactionCondition in _conditions)
            {
                interactionCondition.OnMouseWheel(mouseWheelEventArgs);
            }
        }

        public override bool CanUseSimultaneously
        {
            get
            {
                if (_conditions.Any(x => x.CanUseSimultaneously == false))
                {
                    return false;
                }

                return true;
            }
        }
    }
}