using System.Reactive.Subjects;

namespace BookMap.Presentation.Wpf.InteractionModels
{
    public interface ICapturable : IInteraction
    {
        BehaviorSubject<bool> Captured { get; }
    }
}