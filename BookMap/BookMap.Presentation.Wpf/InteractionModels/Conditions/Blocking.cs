using System;

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

    public class Locked : IInteractionCondition.Decorator
    {
        private bool _isLocked = false;

        public Locked(
            IInteractionCondition interaction) : base(interaction)
        {
        }

        public override bool CanUseSimultaneously => false;

        public override bool IsExclusive => _isLocked;

        public void Lock()
        {
            _isLocked = true;
        }

        public void Unlock()
        {
            _isLocked = false;
        }
    }
}