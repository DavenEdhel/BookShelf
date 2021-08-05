namespace BookMap.Presentation.Apple.Models
{
    public class BoundsDouble
    {
        public static BoundsDouble ImageSize => new BoundsDouble()
        {
            Width = 2560f,
            Height = 1920f
        };

        public double Width { get; set; }

        public double Height { get; set; }

        public FrameDouble Clone()
        {
            return new FrameDouble()
            {
                Height = Height,
                Width = Width
            };
        }

        public BoundsDouble DownScale(double scaleFactor)
        {
            var nWidth = Width / scaleFactor;
            var nHeight = Height / scaleFactor;

            return new BoundsDouble()
            {
                Width = nWidth,
                Height = nHeight
            };
        }
    }
}