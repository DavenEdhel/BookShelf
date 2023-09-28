using System.Windows.Input;

namespace BookMap.Presentation.Wpf.InteractionModels
{
    public class WhenRightMouseButtonClicked : ExecutableInteraction, IInteractionCondition
    {
        public override void OnMouseUp(MouseButtonEventArgs mouseButtonEventArgs)
        {
            if (mouseButtonEventArgs.ChangedButton == MouseButton.Right)
            {
                Captured.OnNext(!Captured.Value);
            }
        }
    }

    public class WhenRightMouseButtonHold : ExecutableInteraction, IInteractionCondition
    {
        public override void OnMouseUp(MouseButtonEventArgs mouseButtonEventArgs)
        {
            if (mouseButtonEventArgs.ChangedButton == MouseButton.Right)
            {
                Captured.OnNext(false);
            }
        }

        public override void OnMouseDown(MouseButtonEventArgs mouseButtonEventArgs)
        {
            base.OnMouseDown(mouseButtonEventArgs);

            if (mouseButtonEventArgs.ChangedButton == MouseButton.Right)
            {
                Captured.OnNext(true);
            }
        }
    }
}