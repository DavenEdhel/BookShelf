using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BookMap.Presentation.Apple.Models;
using BookMap.Presentation.Wpf.Models;

namespace BookMap.Presentation.Wpf.InteractionModels
{
    public class MoveAndScale : ExecutableInteraction
    {
        private readonly Canvas _container;

        private CoordinateSystem _coordinates;
        private readonly IMapView _mapView;

        public MoveAndScale(
            Canvas container,
            CoordinateSystem coordinates,
            IMapView mapView
        )
        {
            _container = container;
            _coordinates = coordinates;
            _mapView = mapView;
        }

        private bool _moveInProgress;
        private Point _initialPoint = new Point(0, 0);
        private Vector _offset = new Vector(0, 0);
        private Point _zoomLocation = new Point(0, 0);

        public override bool CanUseSimultaneously => true;

        public override void OnMouseMove(MouseEventArgs e)
        {
            if (_moveInProgress)
            {
                var current = e.GetPosition(_container);

                _offset = _initialPoint - current;

                _mapView.Reposition(_lastScale, _zoomLocation, _offset);
            }
        }

        public override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (_moveInProgress)
            {
                _coordinates.End();

                _offset = new Vector(0, 0);

                _moveInProgress = false;
            }
        }

        public override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                _coordinates.Begin();

                _moveInProgress = true;

                _initialPoint = e.GetPosition(_container);
            }

            if (e.ChangedButton == MouseButton.Middle)
            {
                _zoomLocation = e.GetPosition(_container);

                _coordinates.Begin();

                _lastScale += 1;

                _mapView.Reposition(_lastScale, _zoomLocation, _offset);

                _coordinates.End();

                _lastScale = 1;
            }
        }

        private double _lastScale = 1;

        public override void OnMouseWheel(MouseWheelEventArgs e)
        {
            _zoomLocation = e.GetPosition(_container);

            _coordinates.Begin();

            _lastScale += e.Delta / 1000f;

            _mapView.Reposition(_lastScale, _zoomLocation, _offset);

            _coordinates.End();

            _lastScale = 1;
        }
    }

    public class AddPin : ExecutableInteraction
    {
        private readonly Canvas _container;

        private CoordinateSystem _coordinates;
        private readonly IMapView _mapView;
        private readonly PinsLayer _pins;

        public AddPin(
            Canvas container,
            CoordinateSystem coordinates,
            IMapView mapView,
            PinsLayer pins
        )
        {
            _container = container;
            _coordinates = coordinates;
            _mapView = mapView;
            _pins = pins;
        }

        public override bool CanUseSimultaneously => true;

        public override void OnMouseUp(MouseButtonEventArgs e)
        {
            var clickPosition = e.GetPosition(_container);

            _pins.Add(
                _coordinates.GetWorldPosition(
                    new PointDouble2D()
                    {
                        X = clickPosition.X,
                        Y = clickPosition.Y
                    }
                )
            );
        }
    }
}