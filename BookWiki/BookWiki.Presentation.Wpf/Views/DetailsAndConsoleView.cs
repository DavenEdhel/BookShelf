using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BookWiki.Core.Files.PathModels;
using BookWiki.Presentation.Wpf.Extensions;
using BookWiki.Presentation.Wpf.Models;
using Keurig.IQ.Core.CrossCutting.Extensions;

namespace BookWiki.Presentation.Wpf.Views
{
    public class DetailsAndConsoleView : StackPanel
    {
        private readonly DetailsView _detailsView;
        private readonly ConsoleView _consoleView;

        public DetailsAndConsoleView()
        {
            VerticalAlignment = VerticalAlignment.Center;

            Children.Add(_detailsView = new DetailsView());
            Children.Add(_consoleView = new ConsoleView());
        }

        public DetailsView Details => _detailsView;

        public void Start()
        {
        }

        public void Stop()
        {
        }

        public void ToggleVisibility(bool? toVisible = null)
        {
            if (toVisible == null)
            {
                if (Visibility == Visibility.Visible)
                {
                    Visibility = Visibility.Collapsed;
                }
                else
                {
                    Visibility = Visibility.Visible;
                }
            }
            else
            {
                Visibility = toVisible.Value ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }
}