using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BookMap.Presentation.Wpf.Models;

namespace BookMap.Presentation.Wpf.InteractionModels
{
    public class Interactions
    {
        private readonly ICapturableInteraction[] _interactions;

        public Interactions(
            Window window,
            Canvas container,
            params ICapturableInteraction[] interactions)
        {
            _interactions = interactions;

            window.KeyDown += OnKeyDown;
            window.KeyUp += OnKeyUp;

            container.MouseWheel += OnMouseWheel;
            container.MouseDown += OnMouseDown;
            container.MouseUp += OnMouseUp;
            container.MouseMove += OnMouseMove;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            ProcessCapture(x => x.OnKeyDown(e));

            UpdateActivationStatus();

            ProcessOnlyCaptured(x => x.OnKeyDown(e));
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            ProcessCapture(x => x.OnKeyUp(e));

            UpdateActivationStatus();

            ProcessOnlyCaptured(x => x.OnKeyUp(e));
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
        }

        private void ProcessCapture(Action<ICapturable> action)
        {
            foreach (var interaction in _interactions)
            {
                action(interaction.Capture);
            }
        }

        private void UpdateActivationStatus()
        {
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