using System.Windows;
using BookMap.Presentation.Apple.Models;
using BookMap.Presentation.Wpf.InteractionModels;
using BookMap.Presentation.Wpf.MapModels.DrawModels;

namespace BookMap.Presentation.Wpf.MapModels
{
    public interface IMapPart
    {
        void Load(ImagePosition position);

        void Save();

        DrawingResult Draw(Point p3, IBrush brush);
    }
}