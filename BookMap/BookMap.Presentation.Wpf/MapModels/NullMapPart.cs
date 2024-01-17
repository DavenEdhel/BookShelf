using System.Windows;
using BookMap.Presentation.Apple.Models;
using BookMap.Presentation.Wpf.InteractionModels;
using BookMap.Presentation.Wpf.MapModels.DrawModels;

namespace BookMap.Presentation.Wpf.MapModels
{
    public class NullMapPart : IMapPart
    {
        public void Load(ImagePosition position)
        {
        }

        public void Save()
        {
        }

        public DrawingResult Draw(Point p3, IBrush brush)
        {
            return new DrawingResult();
        }
    }
}