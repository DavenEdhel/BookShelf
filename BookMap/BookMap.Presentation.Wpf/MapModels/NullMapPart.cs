using System.Windows;
using BookMap.Presentation.Apple.Models;
using BookMap.Presentation.Wpf.InteractionModels;

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

        public void Draw(Point p3, IBrush brush)
        {
        }
    }
}