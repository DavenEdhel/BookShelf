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
using BookMap.Presentation.Apple.Models;
using BookMap.Presentation.Apple.Services;
using BookMap.Presentation.Wpf.Models;

namespace BookMap.Presentation.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MapProviderSynchronous _mapProvider;
        private CoordinateSystem _coordinates;

        private Image[] _activeGround = new Image[9];
        private Image[] _nextGround = new Image[9];
        private Image[] _activeLabels = new Image[9];
        private Image[] _nextLabels = new Image[9];

        private float _lastScale = 1;

        public MainWindow()
        {
            InitializeComponent();

            FileSystemServiceSynchronous.DirectoryPath = @"C:\Work\Projects\BookShelf\BookMap\BookMap.Presentation.Wpf";

            for (int i = 0; i < _nextGround.Length; i++)
            {
                _nextGround[i] = new Image();
                _activeGround[i] = new Image();
                _nextLabels[i] = new Image();
                _activeLabels[i] = new Image();
                
                Container.Children.Add(_nextGround[i]);
                Container.Children.Add(_nextLabels[i]);
                Container.Children.Add(_activeGround[i]);
                Container.Children.Add(_activeLabels[i]);
            }

            Initialize();
        }

        private void Initialize()
        {
            _coordinates = new CoordinateSystem();
            _mapProvider = new MapProviderSynchronous(new WpfImageFactory());

            _coordinates.Reset();

            _mapProvider.LoadMap("1");

            _coordinates.Position(_mapProvider.Settings.Bookmarks.First(x => x.Name == "Аленой"));

            _coordinates.ActualWidthInMeters = _mapProvider.Settings.Width;

            PositionMap();            
        }

        public void PositionMap()
        {
            _coordinates.MoveAndScaleFrame(1, new PointDouble2D(), new PointDouble2D());

            var level0 = _coordinates.GetVisibleItems(_coordinates.DescreetLevel);
            var level1 = _coordinates.GetVisibleItems(_coordinates.DescreetLevel + 1);

            PositionLevel(level0, 0);
            PositionLevel(level1, 1);
        }

        public bool ShowLabels = true;

        private void PositionLevel(ImagePosition[] positions, int level)
        {
            var ground = level == 0 ? _activeGround : _nextGround;
            var label = level == 0 ? _activeLabels : _nextLabels;

            var worldPositions = _coordinates.GetWorldPositions(positions);

            for (int i = 0; i < positions.Length; i++)
            {
                SetPosition(ground[i], worldPositions[i], positions[i]);
                LoadGround(ground[i], positions[i]);

                if (ShowLabels)
                {
                    SetPosition(label[i], worldPositions[i], positions[i]);
                    LoadLabel(label[i], positions[i]);
                }
                else
                {
                    label[i].Source = null;
                }
            }
        }

        private void LoadGround(Image uiImageView, ImagePosition index2D)
        {
            uiImageView.Source = _mapProvider.GetImageAsync(index2D, isLabel: false).ToUIImage();
        }

        private void LoadLabel(Image uiImageView, ImagePosition index2D)
        {
            uiImageView.Source = _mapProvider.GetImageAsync(index2D, isLabel: true).ToUIImage();
        }

        private void SetPosition(Image part, FrameDouble frame, ImagePosition imagePosition)
        {
            if (frame == null)
            {
                part.Opacity = 0;
                return;
            }

            Canvas.SetLeft(part, frame.X);
            Canvas.SetTop(part, frame.Y);
            part.Width = frame.Width;
            part.Height = frame.Height;

            var a = imagePosition.Level - _coordinates.Level;
            var isUpperLevel = a < 0;

            var da = 0.05f;

            var alpha = (isUpperLevel ? ((1 + a) / da) : (1 - a / da));

            part.Opacity = alpha;
        }
    }
}
