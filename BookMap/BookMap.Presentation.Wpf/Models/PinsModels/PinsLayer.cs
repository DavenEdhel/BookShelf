using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Controls;
using BookMap.Presentation.Apple.Models;
using BookMap.Presentation.Apple.Services;

namespace BookMap.Presentation.Wpf.Models
{
    // todo: pins visibility
    //  DisplayOnlyCurrentLevelPins
    public class PinsLayer : Canvas, IPins
    {
        private readonly CoordinateSystem _coordinates;
        private readonly IPinsFeature _pinsFeature;
        private readonly MapProviderSynchronous _mapProvider;

        private readonly List<Pin> _pins = new();

        public Subject<Unit> Changed { get; } = new();

        public PinsLayer(CoordinateSystem coordinates, IPinsFeature pinsFeature, MapProviderSynchronous mapProvider)
        {
            _coordinates = coordinates;
            _pinsFeature = pinsFeature;
            _mapProvider = mapProvider;
        }

        public void Load()
        {
            foreach (var settingsPin in _mapProvider.Settings.Pins)
            {
                Render(new Pin(this)
                {
                    Data = settingsPin,
                    View = _pinsFeature.CreateView(settingsPin)
                });
            }
        }

        public void Reposition()
        {
            foreach (var pin in _pins)
            {
                var c = _coordinates.GetWorldPoint(pin.Data.Position);

                Canvas.SetLeft(pin.View, c.X);
                Canvas.SetTop(pin.View, c.Y);
            }
        }

        public void Add(ImagePositionDouble position)
        {
            if (_pinsFeature.CanCreateNew == false)
            {
                return;
            }

            var data = new PinDto()
            {
                Id = Guid.NewGuid()
            };
            var p = _pinsFeature.NewPin();
            data.Name = p.Name;
            data.Payload = p.Payload;
            data.Position = position;

            var view = _pinsFeature.CreateView(data);

            var pin = new Pin(this)
            {
                Data = data,
                View = view
            };

            Render(pin);

            _mapProvider.AddPin(data);

            Reposition();

            Changed.OnNext(Unit.Default);
        }

        public void Remove(Guid pinId)
        {
            (UIElement e, Pin pin) = Get(pinId);

            if (e != null)
            {
                Children.Remove(e);
            }

            _pins.Remove(pin);

            _mapProvider.RemovePin(pinId);

            Changed.OnNext(Unit.Default);
        }

        public void Highlight(Guid pinId)
        {
            foreach (var pin in _pins)
            {
                _pinsFeature.SetNormal(pin.View);
            }

            _pinsFeature.SetHighlighted(Get(pinId).Item1);
        }

        public void ClearHighlighting()
        {
            foreach (var pin in _pins)
            {
                _pinsFeature.SetNormal(pin.View);
            }
        }

        public bool Visible
        {
            get => Visibility == Visibility.Visible;
            set
            {
                Visibility = value ? Visibility.Visible : Visibility.Hidden;
            }
        }

        private void Render(Pin pin)
        {
            _pins.Add(pin);

            Children.Add(pin.View);
        }

        private (UIElement, Pin) Get(Guid pinId)
        {
            var pin = _pins.First(x => x.Data.Id == pinId);

            UIElement e = null;
            foreach (UIElement child in Children)
            {
                if (ReferenceEquals(child, pin.View))
                {
                    e = child;
                    break;
                }
            }

            return (e, pin);
        }

        public IEnumerator<IPin> GetEnumerator()
        {
            return _pins.ToArray().ToList().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}