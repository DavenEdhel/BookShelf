using System.Windows;
using BookMap.Presentation.Apple.Services;

namespace BookMap.Presentation.Wpf.Models
{
    public class Pin : IPin
    {
        private readonly PinsLayer _layer;

        public Pin(PinsLayer layer)
        {
            _layer = layer;
        }

        public PinDto Data { get; set; }

        public UIElement View { get; set; }

        public void Highlight()
        {
            _layer.Highlight(Data.Id);
        }
    }
}