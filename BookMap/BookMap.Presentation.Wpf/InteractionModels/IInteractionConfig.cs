﻿namespace BookMap.Presentation.Wpf.InteractionModels
{
    public interface IInteractionConfig
    {
        bool CanUseSimultaneously { get; }

        bool IsExclusive { get; }
    }
}