using System.Windows.Controls;
using System.Windows.Input;
using BookMap.Presentation.Apple.Models;

namespace BookMap.Presentation.Wpf.InteractionModels
{
    public class Measure : ExecutableInteraction
    {
        private readonly ILabel _label;
        private readonly Canvas _container;
        private readonly Models.Measure _measure;

        public Measure(
            ILabel label,
            Canvas container,
            Models.Measure measure
        )
        {
            _label = label;
            _container = container;
            _measure = measure;
        }

        private bool _measureMode;

        public override bool CanUseSimultaneously => false;

        public override void OnKeyUp(KeyEventArgs e)
        {
            if (!e.IsDown && e.Key == Key.LeftCtrl)
            {
                if (_measureMode)
                {
                    _measure.Reset();
                }

                _label.Set($"Measure on {_measure.Meters}m");
            }

            if (!e.IsDown && e.Key == Key.LeftShift)
            {
                _label.Set($"Measure off {_measure.Meters}m");
            }
        }

        public override void OnMouseUp(MouseButtonEventArgs e)
        {
            var position = e.GetPosition(_container);

            _measure.AddPoint(new PointDouble2D()
            {
                X = position.X,
                Y = position.Y
            });

            _label.Set($"Measure {_measure.Meters}m");
        }
    }
}