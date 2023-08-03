namespace BookMap.Presentation.Wpf.InteractionModels
{
    public class Interaction : ICapturableInteraction
    {
        private readonly IInteractionCondition _condition;
        private readonly IExecutableInteraction _behavior;

        public Interaction(IInteractionCondition condition, IExecutableInteraction behavior)
        {
            _condition = condition;
            _behavior = behavior;
        }

        public IInteractionCondition Capture => _condition;

        public IExecutableInteraction Behavior => _behavior;
    }
}