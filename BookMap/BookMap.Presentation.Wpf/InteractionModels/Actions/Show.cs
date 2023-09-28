﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BookMap.Presentation.Wpf.InteractionModels
{
    public class Show : ExecutableInteraction
    {
        private readonly UIElement _element;
        private readonly Canvas _container;
        private Point _position;
        private bool _positioned = false;

        public Show(
            UIElement element,
            Canvas container)
        {
            _element = element;
            _container = container;
            _element.Visibility = Visibility.Collapsed;

            IsActive.Subscribe(
                isActive =>
                {
                    if (isActive)
                    {
                        _element.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        _element.Visibility = Visibility.Collapsed;
                        _positioned = false;
                    }
                }
            );

            IsActive.OnNext(false);
            Captured.OnNext(false);
        }

        public override bool CanUseSimultaneously => true;

        public override bool IsBackground => false;

        public override void OnMouseUp(MouseButtonEventArgs mouseButtonEventArgs)
        {
            _position = mouseButtonEventArgs.GetPosition(_container);
            if (_positioned == false)
            {
                Canvas.SetLeft(_element, _position.X + 10);
                Canvas.SetTop(_element, _position.Y + 10);
                _positioned = true;
            }
        }
    }
}