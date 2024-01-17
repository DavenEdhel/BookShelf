namespace BookMap.Presentation.Apple.Models
{
    public class ImagePosition
    {
        public int Level { get; set; }

        public int X { get; set; }

        public int Y { get; set; }

        public bool EqualTo(ImagePosition second)
        {
            return X == second.X && Y == second.Y && Level == second.Level;
        }

        public ImagePosition Right()
        {
            return new ImagePosition()
            {
                Level = Level,
                X = X + 1,
                Y = Y
            };
        }

        public ImagePosition Left()
        {
            return new ImagePosition()
            {
                Level = Level,
                X = X - 1,
                Y = Y
            };
        }

        public ImagePosition Up()
        {
            return new ImagePosition()
            {
                Level = Level,
                X = X,
                Y = Y - 1
            };
        }

        public ImagePosition Down()
        {
            return new ImagePosition()
            {
                Level = Level,
                X = X,
                Y = Y + 1
            };
        }

        public ImagePosition UpperLevel()
        {
            return new ImagePosition()
            {
                Level = Level - 1,
                X = X/8,
                Y = Y/8
            };
        }

        public ImagePosition RelativePositionToParent()
        {
            return new ImagePosition()
            {
                Level = 1,
                X = X % 8,
                Y = Y % 8
            };
        }

        public override string ToString()
        {
            return $"[{Level},{X},{Y}]";
        }
    }
}