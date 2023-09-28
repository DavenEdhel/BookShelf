namespace BookMap.Presentation.Wpf.InteractionModels
{
    public class AlwaysOn : ExecutableInteraction, IInteractionCondition
    {
        public AlwaysOn()
        {
            Captured.OnNext(true);
        }
    }
}