using System.Windows;
using BookMap.Presentation.Apple.Models;
using BookMap.Presentation.Wpf.InteractionModels;

namespace BookMap.Presentation.Wpf.MapModels
{
    public interface IMapPart
    {
        void Load(ImagePosition position);

        void Save();

        void Draw(Point p3, IBrush brush);
    }
}