using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BookMap.Presentation.Wpf.Models;

namespace BookMap.Presentation.Wpf.InteractionModels
{
    public class Interactions
    {
        private readonly Window _window;
        private readonly Canvas _container;
        private ICapturableInteraction[] _interactions;
        private readonly List<UIElement> _allowed = new();
        private bool _isActive;

        public Interactions(
            Window window,
            Canvas container)
        {
            _window = window;
            _container = container;
        }

        public void Add(params ICapturableInteraction[] interactions)
        {
            _interactions = interactions;

            _window.KeyDown += OnKeyDown;
            _window.KeyUp += OnKeyUp;

            _container.MouseEnter += Activate;
            _container.MouseLeave += Deactivate;

            _container.MouseWheel += OnMouseWheel;
            _container.MouseDown += OnMouseDown;
            _container.MouseUp += OnMouseUp;
            _container.MouseMove += OnMouseMove;
        }

        public void RegisterForInteraction(UIElement element)
        {
            _allowed.Add(element);
        }

        public void UnregisterFromInteraction(UIElement element)
        {
            _allowed.Remove(element);
        }

        private void Activate(object sender, MouseEventArgs e)
        {
            _isActive = true;
        }

        private void Deactivate(object sender, MouseEventArgs e)
        {
            _isActive = false;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            ProcessCapture(x => x.OnKeyDown(e));

            UpdateActivationStatus();

            ProcessOnlyCaptured(x => x.OnKeyDown(e));

            if (_isActive)
            {
                if (_allowed.Any(x => x.IsFocused) == false)
                {
                    e.Handled = true;
                }
            }
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            ProcessCapture(x => x.OnKeyUp(e));

            UpdateActivationStatus();

            ProcessOnlyCaptured(x => x.OnKeyUp(e));

            if (_isActive)
            {
                if (_allowed.Any(x => x.IsFocused) == false)
                {
                    e.Handled = true;
                }
            }
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            ProcessCapture(x => x.OnMouseDown(e));

            UpdateActivationStatus();

            ProcessOnlyCaptured(x => x.OnMouseDown(e));
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            ProcessCapture(x => x.OnMouseMove(e));

            UpdateActivationStatus();

            ProcessOnlyCaptured(x => x.OnMouseMove(e));
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            ProcessCapture(x => x.OnMouseUp(e));

            UpdateActivationStatus();

            ProcessOnlyCaptured(x => x.OnMouseUp(e));
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ProcessCapture(x => x.OnMouseWheel(e));

            UpdateActivationStatus();

            ProcessOnlyCaptured(x => x.OnMouseWheel(e));

            if (_isActive)
            {
                e.Handled = true;
            }
        }

        private void ProcessCapture(Action<ICapturable> action)
        {
            if (_isActive == false)
            {
                return;
            }

            if (_interactions.Any(x => x.Capture.IsExclusive && x.Capture.Captured.Value))
            {
                return;
            }

            foreach (var interaction in _interactions)
            {
                action(interaction.Capture);
            }
        }

        private void UpdateActivationStatus()
        {
            if (_isActive == false)
            {
                return;
            }

            if (_interactions.Any(x => x.Capture.IsExclusive && x.Capture.Captured.Value))
            {
                return;
            }

            var captureFound = false;

            foreach (var interaction in _interactions)
            {
                if (interaction.Capture.Captured.Value)
                {
                    if (!captureFound)
                    {
                        interaction.Behavior.IsActive.OnNext(true);

                        if (interaction.Behavior.IsBackground == false)
                        {
                            captureFound = true;
                        }
                    }
                    else
                    {
                        interaction.Behavior.IsActive.OnNext(false);
                    }
                }
                else
                {
                    interaction.Behavior.IsActive.OnNext(false);
                }
            }
        }

        private void ProcessOnlyCaptured(Action<IExecutableInteraction> action)
        {
            if (_isActive == false)
            {
                return;
            }

            if (_interactions.Any(x => x.Capture.IsExclusive && x.Capture.Captured.Value))
            {
                foreach (var capturableInteraction in _interactions.Where(x => x.Capture.IsExclusive && x.Capture.Captured.Value))
                {
                    action(capturableInteraction.Behavior);
                }

                return;
            }

            foreach (var interaction in _interactions)
            {
                if (interaction.Capture.Captured.Value)
                {
                    action(interaction.Behavior);

                    if (interaction.Capture.CanUseSimultaneously == false)
                    {
                        return;
                    }
                }
            }
        }
    }
}