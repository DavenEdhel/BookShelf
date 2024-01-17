using System;
using System.Reactive.Linq;

namespace BookMap.Presentation.Wpf.InteractionModels
{
    public class WithStatus : IExecutableInteraction.Decorator
    {
        public WithStatus(
            string featureName,
            ILabel label,
            IExecutableInteraction interaction) : base(interaction)
        {
            interaction.IsActive.Skip(1).Subscribe(
                isActive =>
                {
                    if (isActive)
                    {
                        label.Set($"{featureName} is turned ON");
                    }
                }
            );
        }
    }
}