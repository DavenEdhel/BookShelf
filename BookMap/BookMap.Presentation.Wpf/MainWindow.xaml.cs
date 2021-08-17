using System;
using System.Collections.Generic;
using System.IO;
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
using Newtonsoft.Json;

namespace BookMap.Presentation.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MapProviderSynchronous _mapProvider;
        private CoordinateSystem _coordinates;
        private Measure _measure;

        private Image[] _activeGround = new Image[9];
        private Image[] _nextGround = new Image[9];
        private Image[] _activeLabels = new Image[9];
        private Image[] _nextLabels = new Image[9];

        private double _lastScale = 1;

        public MainWindow()
        {
            InitializeComponent();

            _config = JsonConvert.DeserializeObject<AppConfigDto>(File.ReadAllText("MapConfig.json"));

            FileSystemServiceSynchronous.DirectoryPath = _config.Root;

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

            _menu = new StackPanel();
            _menu.Width = Width;
            _menu.Height = 20;

            Container.Children.Add(_menu);

            _menu.Children.Add(_info = new TextBlock());
            _info.Text = _lastScale.ToString();

            Container.MouseWheel += ContainerOnMouseWheel;
            Container.MouseDown += ContainerOnMouseDown;
            Container.MouseUp += ContainerOnMouseUp;
            Container.MouseMove += ContainerOnMouseMove;

            KeyUp += OnKeyUp;

            Initialize();
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (!e.IsDown && e.Key == Key.LeftCtrl)
            {
                if (_measureMode)
                {
                    _measure.Reset();
                }

                _measureMode = true;

                Title = $"Measure on {_measure.Meters}m";
            }

            if (!e.IsDown && e.Key == Key.LeftShift)
            {
                _measureMode = false;

                Title = $"Measure off {_measure.Meters}m";
            }
        }

        private void ContainerOnMouseMove(object sender, MouseEventArgs e)
        {
            if (_moveInProgress)
            {
                var current = e.GetPosition(Container);

                _offset = _initialPoint - current;

                PositionMap();
            }
        }

        private void ContainerOnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_measureMode)
            {
                var position = e.GetPosition(Container);

                _measure.AddPoint(new PointDouble2D()
                {
                    X = position.X,
                    Y = position.Y
                });

                Title = $"Measure {_measure.Meters}m";
            }

            if (_moveInProgress)
            {
                _coordinates.End();

                _offset = new Vector(0, 0);

                _moveInProgress = false;
            }
        }

        private void ContainerOnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_measureMode)
            {
                return;
            }

            if (e.ChangedButton == MouseButton.Left)
            {
                _coordinates.Begin();

                _moveInProgress = true;

                _initialPoint = e.GetPosition(Container);
            }

            if (e.ChangedButton == MouseButton.Middle)
            {
                _zoomLocation = e.GetPosition(Container);

                _coordinates.Begin();

                _lastScale += 1;

                PositionMap();

                _coordinates.End();

                _lastScale = 1;
            }
        }

        private void ContainerOnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            _zoomLocation = e.GetPosition(Container);

            _coordinates.Begin();

            _lastScale += e.Delta/1000f;

            PositionMap();

            _coordinates.End();

            _lastScale = 1;
        }

        private void Initialize()
        {
            _coordinates = new CoordinateSystem();
            _mapProvider = new MapProviderSynchronous(new WpfImageFactory());
            _measure = new Measure(_coordinates);

            _coordinates.Reset();

            _mapProvider.LoadMap("1");

            //_coordinates.Position(_mapProvider.Settings.Bookmarks.First(x => x.Name == "Аленой"));

            _coordinates.ActualWidthInMeters = _mapProvider.Settings.Width;

            PositionMap();            
        }

        public void PositionMap()
        {
            _coordinates.MoveAndScaleFrame(_lastScale, new PointDouble2D()
            {
                X = _zoomLocation.X,
                Y = _zoomLocation.Y
            },
                new PointDouble2D()
            {
                X = -_offset.X,
                Y = -_offset.Y
            });

            var level0 = _coordinates.GetVisibleItems(_coordinates.DescreetLevel);
            var level1 = _coordinates.GetVisibleItems(_coordinates.DescreetLevel + 1);

            PositionLevel(level0, 0);
            PositionLevel(level1, 1);
        }

        public bool ShowLabels = true;
        private StackPanel _menu;
        private TextBlock _info;
        private bool _moveInProgress;
        private Point _initialPoint = new Point(0, 0);
        private Vector _offset = new Vector(0, 0);
        private Point _zoomLocation = new Point(0, 0);
        private bool _measureMode;
        private AppConfigDto? _config;

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
