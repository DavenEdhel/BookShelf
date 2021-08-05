namespace BookMap.Presentation.Apple.Models
{
    public class FrameDouble
    {
        public double X { get; set; }
        
        public double Y { get; set; }

        public double Width { get; set; }

        public double Height { get; set; }

        public FrameDouble()
        {
        }

        public FrameDouble(BoundsDouble bounds)
        {
            Width = bounds.Width;
            Height = bounds.Height;
        }

        public FrameDouble Clone()
        {
            return new FrameDouble()
            {
                X = X,
                Y = Y,
                Height = Height,
                Width = Width
            };
        }

        public FrameDouble Scale(double scaleFactor)
        {
            var nWidth = Width * scaleFactor;
            var nHeight = Height * scaleFactor;

            return new FrameDouble()
            {
                X = X,
                Y = Y,
                Width = nWidth,
                Height = nHeight
            };
        }

        public FrameDouble Left(double d)
        {
            var cloned = Clone();
            cloned.X -= d;

            return cloned;
        }

        public FrameDouble Down(double d)
        {
            var cloned = Clone();
            cloned.Y += d;

            return cloned;
        }

        public FrameDouble Up(double d)
        {
            var cloned = Clone();
            cloned.Y -= d;

            return cloned;
        }

        public FrameDouble Right(double d)
        {
            var cloned = Clone();
            cloned.X += d;

            return cloned;
        }
    }
}