namespace BookMap.Presentation.Apple.Services
{
    public class BrushInfo
    {
        public float Size { get; set; } = 3;

        public string Color { get; set; } = "#000000";

        public BrushInfo(float size, string color)
        {
            Size = size;
            Color = color;
        }

        public BrushInfo()
        {
        }
    }
}