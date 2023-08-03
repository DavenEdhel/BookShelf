using System.Reactive.Subjects;

namespace BookMap.Presentation.Wpf.InteractionModels
{
    public interface IActive : IInteraction
    {
        BehaviorSubject<bool> IsActive { get; }
    }
}