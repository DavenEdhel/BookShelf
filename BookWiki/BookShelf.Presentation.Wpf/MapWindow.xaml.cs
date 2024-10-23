using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using BookWiki.Core;
using BookWiki.Core.Articles;
using BookWiki.Core.Files.FileModels;
using BookWiki.Core.Files.PathModels;
using BookWiki.Core.FileSystem.FileModels;
using BookWiki.Core.LifeSpellCheckModels;
using BookWiki.Core.Logging;
using BookWiki.Core.Utils.TextModels;
using BookWiki.Presentation.Wpf;
using BookWiki.Presentation.Wpf.Models;
using BookWiki.Presentation.Wpf.Models.PinsModels;
using BookWiki.Presentation.Wpf.Models.QuickNavigationModels;
using BookWiki.Presentation.Wpf.Models.SpellCheckModels;
using BookWiki.Presentation.Wpf.Views;
using Keurig.IQ.Core.CrossCutting.Extensions;
using Path = System.Windows.Shapes.Path;

namespace BookShelf.Presentation.Wpf
{
    /// <summary>
    /// todo:
    ///  create new map
    ///  change map settings
    ///  map reference from article with option to go to map view
    /// </summary>
    public partial class MapWindow : Window
    {
        public MapWindow(IRelativePath mapPath)
        {
            MapPath = mapPath;

            InitializeComponent();

            Map.InitPins(new PinAsArticleLinkFeature());
            Map.Init(this, mapPath.AbsolutePath(BooksApplication.Instance.RootPath).FullPath);

            _openedTabs = new OpenedTabsView();
            _openedTabs.Width = 300;
            _openedTabs.HorizontalAlignment = HorizontalAlignment.Left;
            _openedTabs.Visibility = Visibility.Visible;
            _openedTabs.Margin = new Thickness(0);
            _openedTabs.MouseDown += ChangeOpenedTabsVisibility;
            Grid.SetColumn(_openedTabs, 0);
            Root.Children.Add(_openedTabs);

            Title = new MapTitle(mapPath).PlainText;

            _openedTabs.Start();

            BookmarksView.Start(Map.Bookmarks);
            PinsView.Start(Map.Pins, Map);
        }

        public IRelativePath MapPath { get; set; }

        private readonly Logger _logger = new Logger(nameof(NovelWindow));
        private readonly OpenedTabsView _openedTabs;

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            Map.CleanUp();
            _openedTabs.Stop();
            BookmarksView.Stop();
            PinsView.Stop();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            BooksApplication.Instance.ReportWindowClosed();
        }

        private void NovelWindow_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (BooksApplication.Instance.KeyProcessor.Handle(e.KeyboardDevice))
            {
                e.Handled = true;
                return;
            }
        }

        private void TopBarMouseDown(object sender, MouseButtonEventArgs e)
        {
            TopBarGrid.Visibility = (TopBarGrid.Visibility == Visibility.Visible) ? Visibility.Hidden : Visibility.Visible;
        }

        private bool CanTabsBeVisible(double width) => true;

        private void ChangeOpenedTabsVisibility(object sender, MouseButtonEventArgs e)
        {
            if (CanTabsBeVisible(ActualWidth))
            {
                _openedTabs.ToggleVisibility();
            }
        }

        private void PinsSwitch(object sender, RoutedEventArgs e)
        {
            TogglePins();

            //BooksApplication.Instance.PageConfig.SetScrollVisibility(ScrollSwitchButton.Content.ToString() == "Scroll Visible");

            // todo: pins visibility persistance - move into maps config and make it configurable there as well as other settings
        }

        private void TogglePins()
        {
            if (ArePinsVisible())
            {
                ScrollSwitchButton.Content = "Pins Hidden";

                Map.Pins.Visible = false;
            }
            else
            {
                ScrollSwitchButton.Content = "Pins Visible";

                Map.Pins.Visible = true;
            }
        }

        private bool ArePinsVisible()
        {
            return ScrollSwitchButton.Content.ToString() == "Pins Visible";
        }
    }
}
