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
    }
}