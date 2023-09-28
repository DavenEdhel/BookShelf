namespace BookMap.Presentation.Wpf.InteractionModels
{
    public class Blocking : IInteractionCondition.Decorator
    {
        public Blocking(
            IInteractionCondition interaction) : base(interaction)
        {
        }

        public override bool CanUseSimultaneously => false;
    }
}