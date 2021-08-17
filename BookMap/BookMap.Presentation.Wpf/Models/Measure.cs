using BookMap.Presentation.Apple.Models;

namespace BookMap.Presentation.Wpf.Models
{
    public class Measure
    {
        private readonly CoordinateSystem _coordinates;

        private PointDouble2D _last = null;

        public Measure(CoordinateSystem coordinates)
        {
            _coordinates = coordinates;
        }

        public double Meters { get; set; } = 0;

        public void Reset()
        {
            Meters = 0;
            _last = null;
        }

        public void AddPoint(PointDouble2D point)
        {
            if (_last == null)
            {
                _last = point;
            }
            else
            {
                Meters += _coordinates.GetDistance(_last, point);

                _last = point;
            }
        }
    }
}