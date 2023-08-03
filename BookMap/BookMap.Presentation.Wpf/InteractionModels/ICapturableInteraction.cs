namespace BookMap.Presentation.Wpf.InteractionModels
{
    public interface ICapturableInteraction
    {
        IInteractionCondition Capture { get; }

        IExecutableInteraction Behavior { get; }
    }
}