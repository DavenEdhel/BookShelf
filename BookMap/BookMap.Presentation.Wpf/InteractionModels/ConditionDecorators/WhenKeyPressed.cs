using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace BookMap.Presentation.Wpf.InteractionModels
{
    public class WhenKeyPressed : ExecutableInteraction, IInteractionCondition
    {
        private readonly Key _activationKey;
        private readonly List<Key> _deactivation;

        public WhenKeyPressed(Key activationKey, List<Key> deactivation = null)
        {
            _activationKey = activationKey;
            _deactivation = deactivation ?? new List<Key>();
        }

        public override void OnKeyUp(KeyEventArgs e)
        {
            if (!e.IsDown && e.Key == _activationKey)
            {
                Captured.OnNext(!Captured.Value);
            }
            else if(!e.IsDown && _deactivation.Contains(e.Key))
            {
                Captured.OnNext(false);
            }

            base.OnKeyUp(e);
        }
    }

    public class DependingOnKey : ExecutableInteraction, IInteractionCondition
    {
        private readonly Key _activation;
        private readonly Key[] _deactivation;

        public DependingOnKey(
            Key activation,
            Key[] deactivation = null
            )
        {
            _activation = activation;
            _deactivation = deactivation;
        }

        public override void OnKeyUp(KeyEventArgs e)
        {
            if (!e.IsDown && e.Key == _activation)
            {
                Captured.OnNext(!Captured.Value);
            }
            else if (!e.IsDown && _deactivation.Contains(e.Key))
            {
                Captured.OnNext(false);
            }

            base.OnKeyUp(e);
        }
    }
}