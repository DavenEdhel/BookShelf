using System.Windows;

namespace BookMap.Presentation.Wpf.InteractionModels
{
    public interface IMapView
    {
        void Reposition(double lastScale, Point zoomLocation, Vector offset);
    }
}