using UIKit;

namespace BookMap.Presentation.Apple.Views
{
    public class Brush
    {
        public float Size { get; set; }

        public UIColor Color { get; set; }

        public bool IsEraser { get; set; }

        public static readonly Brush Eraser = new Brush(5, UIColor.Black)
        {
            IsEraser = true
        };

        public Brush(float size, UIColor color)
        {
            Size = size;
            Color = color;
        }
    }
}