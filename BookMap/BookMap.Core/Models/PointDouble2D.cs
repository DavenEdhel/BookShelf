namespace BookMap.Presentation.Apple.Models
{
    public class PointDouble2D
    {
        public double X { get; set; }

        public double Y { get; set; }

        public PointDouble2D Substract(PointDouble2D b)
        {
            return new PointDouble2D()
            {
                X = X - b.X,
                Y = Y - b.Y
            };
        }

        public PointDouble2D Multiply(double scale)
        {
            return new PointDouble2D()
            {
                X = X * scale,
                Y = Y * scale
            };
        }

        public override string ToString()
        {
            return $"[{X};{Y}]";
        }
    }
}