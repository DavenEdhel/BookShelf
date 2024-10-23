using System;
using System.Reactive.Linq;
using BookMap.Presentation.Wpf.Views;

namespace BookMap.Presentation.Wpf.InteractionModels
{
    public class WithStatus : IExecutableInteraction.Decorator
    {
        public WithStatus(
            string featureName,
            ILabel label,
            IExecutableInteraction interaction) : base(interaction)
        {
            interaction.IsActive.Skip(1).DistinctUntilChanged().Subscribe(
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

    public class WithPalette : IExecutableInteraction.Decorator
    {
        private readonly Palettes _palettes;

        public WithPalette(
            string featureName,
            Palettes palettes,
            Palette palette,
            IExecutableInteraction interaction) : base(interaction)
        {
            _palettes = palettes;
            _palettes.Register(featureName, palette);
            interaction.IsActive.Skip(1).DistinctUntilChanged().Subscribe(
                isActive =>
                {
                    if (isActive)
                    {
                        _palettes.Activate(featureName);
                    }
                }
            );
        }
    }
}