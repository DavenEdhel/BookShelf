using System.Windows.Input;

namespace BookMap.Presentation.Wpf.InteractionModels
{
    public class WhenKeyPressed : ExecutableInteraction, IInteractionCondition
    {
        private readonly Key _activationKey;

        public WhenKeyPressed(Key activationKey)
        {
            _activationKey = activationKey;
        }

        public override void OnKeyUp(KeyEventArgs e)
        {
            if (!e.IsDown && e.Key == _activationKey)
            {
                Captured.OnNext(!Captured.Value);
            }

            if (!e.IsDown && e.Key != _activationKey)
            {
                Captured.OnNext(false);
            }

            base.OnKeyUp(e);
        }
    }
}