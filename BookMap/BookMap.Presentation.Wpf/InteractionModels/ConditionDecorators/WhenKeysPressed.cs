using System.Windows.Input;

namespace BookMap.Presentation.Wpf.InteractionModels
{
    public class WhenKeysPressed : ExecutableInteraction, IInteractionCondition
    {
        private readonly Key _on;
        private readonly Key _off;

        public WhenKeysPressed(
            Key on,
            Key off)
        {
            _on = on;
            _off = off;
        }

        public override void OnKeyUp(KeyEventArgs e)
        {
            if (!e.IsDown && e.Key == Key.LeftCtrl)
            {
                Captured.OnNext(true);
            }

            if (!e.IsDown && e.Key == Key.LeftShift)
            {
                Captured.OnNext(false);
            }
        }
    }
}