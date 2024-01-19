using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using BookWiki.Core.Files.FileModels;

namespace BookWiki.Presentation.Wpf.Views
{
    public static class VisibilityExtensions
    {
        public static Visibility ToVisibility(this bool? value, Visibility current)
        {
            if (value == null)
            {
                if (current == Visibility.Visible)
                {
                    return Visibility.Collapsed;
                }
                else
                {
                    return Visibility.Visible;
                    BooksApplication.Instance.RightSideBarConfig.Change(x => x.IsVisible = true);
                }
            }
            else
            {
                return value.Value ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }

    public partial class RightSideViewV2 : UserControl
    {
        public RightSideViewV2()
        {
            InitializeComponent();

            Panel.SizeChanged += PanelOnSizeChanged;
        }

        private void PanelOnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (ConsoleView.Visibility == Visibility.Visible && DetailsView.Visibility == Visibility.Collapsed)
            {
                ConsoleView.SetHeight(Panel.ActualHeight);
            }

            if (ConsoleView.Visibility == Visibility.Collapsed && DetailsView.Visibility == Visibility.Visible)
            {
                DetailsView.Height = Panel.ActualHeight;
            }
        }

        public DetailsView Details => DetailsView;

        public void Start()
        {
            Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);

            BooksApplication.Instance.RightSideBarConfig.Changed += RightSideBarConfigOnChanged;

            RightSideBarConfigOnChanged(BooksApplication.Instance.RightSideBarConfig.Current);

            ConsoleView.Start();
        }

        private void RightSideBarConfigOnChanged(RightSideBarSettings obj)
        {
            ToggleVisibility(toVisible: obj.IsVisible);
            ToggleDetailsView(toVisible: obj.IsCommentBarVisible);
            ToggleConsoleView(toVisible: obj.IsConsoleBarHidden);
        }

        private static Action EmptyDelegate = delegate () { };

        public void Stop()
        {
            BooksApplication.Instance.RightSideBarConfig.Changed -= RightSideBarConfigOnChanged;

            ConsoleView.Stop();
        }

        public void ToggleVisibility(bool? toVisible = null)
        {
            var visibility = toVisible.ToVisibility(Visibility);

            Visibility = visibility;

            BooksApplication.Instance.RightSideBarConfig.Change(x => x.IsVisible = visibility == Visibility.Visible);
        }

        private void ToggleDetailsView(object sender, RoutedEventArgs routedEventArgs)
        {
            ToggleDetailsView();
        }

        private void ToggleDetailsView(bool? toVisible = null)
        {
            DetailsView.Visibility = toVisible.ToVisibility(DetailsView.Visibility);

            if (DetailsView.Visibility == Visibility.Collapsed)
            {
                ConsoleView.SetHeight(Panel.ActualHeight);
            }
            else
            {
                ConsoleView.SetHeight(450);
            }

            BooksApplication.Instance.RightSideBarConfig.Change(x => x.IsCommentBarVisible = DetailsView.Visibility == Visibility.Visible);
        }

        private void ToggleConsoleView(object sender, RoutedEventArgs e)
        {
            ToggleConsoleView();
        }

        private void ToggleConsoleView(bool? toVisible = null)
        {
            ConsoleView.Visibility = toVisible.ToVisibility(ConsoleView.Visibility);

            if (ConsoleView.Visibility == Visibility.Collapsed)
            {
                DetailsView.Height = Panel.ActualHeight;
            }
            else
            {
                DetailsView.Height = 450;
            }

            BooksApplication.Instance.RightSideBarConfig.Change(x => x.IsConsoleBarHidden = ConsoleView.Visibility == Visibility.Visible);
        }
    }
}
