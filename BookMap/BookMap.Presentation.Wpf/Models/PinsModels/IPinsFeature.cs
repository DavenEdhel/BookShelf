using System.Windows;
using BookMap.Presentation.Apple.Services;

namespace BookMap.Presentation.Wpf.Models
{
    public interface IPinsFeature
    {
        bool CanCreateNew { get; }

        PinInfo NewPin();

        UIElement CreateView(PinDto info);

        void SetNormal(UIElement view);

        void SetHighlighted(UIElement view);
    }
}