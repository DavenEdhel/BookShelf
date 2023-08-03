using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using BookMap.Core.Models;
using BookMap.Presentation.Apple.Models;
using BookMap.Presentation.Apple.Services;
using BookMap.Presentation.Wpf.InteractionModels;
using BookMap.Presentation.Wpf.MapModels;
using BookMap.Presentation.Wpf.Models;
using BookMap.Presentation.Wpf.Views;
using Newtonsoft.Json;
using MapInfo = BookMap.Presentation.Apple.Services.MapInfo;
using Measure = BookMap.Presentation.Wpf.InteractionModels.Measure;
using Path = System.IO.Path;

namespace BookMap.Presentation.Wpf
{
    //todo:
    // useful api to draw on image
    // pointer for moving, pointer for drawing, pointer for labels, they should be different
    //  pointer should correspond drawing area
    //  should have borders of colors of drawing or semitransparent
    //  ground or label on it with the color of drawing
    // palette, try to use wpf one, but not necessary
    // ability to create and fast configure new map
    
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IMapView, ILabel
    {
        private int w = 2560;
        private int h = 1920;

        private MapProviderSynchronous _mapProvider;
        private CoordinateSystem _coordinates;
        private Models.Measure _measure;

        private MapPart[] _activeGround = new MapPart[9];
        private MapPart[] _nextGround = new MapPart[9];
        private MapPart[] _activeLabels = new MapPart[9];
        private MapPart[] _nextLabels = new MapPart[9];

        private MapLayer _ground;

        public bool ShowLabels = true;
        private AppConfigDto? _config;
        
        private readonly MapLayer _labels;
        private readonly Palette _palette;
        private readonly Interactions _interactions;
        private readonly CurrentBrush _brush;

        public MainWindow()
        {
            InitializeComponent();

            _coordinates = new CoordinateSystem();
            _mapProvider = new MapProviderSynchronous(new WpfImageFactory());
            _measure = new Models.Measure(_coordinates);

            _config = JsonConvert.DeserializeObject<AppConfigDto>(File.ReadAllText("MapConfig.json"));

            FileSystemServiceSynchronous.DirectoryPath = _config.Root;

            var mapInfo = new MapReference(_config, "1");

            for (int i = 0; i < _nextGround.Length; i++)
            {
                _nextGround[i] = new MapPart(mapInfo, isLabel: false, _mapProvider);
                _activeGround[i] = new MapPart(mapInfo, isLabel: false, _mapProvider);
                _nextLabels[i] = new MapPart(mapInfo, isLabel: true, _mapProvider);
                _activeLabels[i] = new MapPart(mapInfo, isLabel: true, _mapProvider);

                Container.Children.Add(_nextGround[i]);
                Container.Children.Add(_nextLabels[i]);
                Container.Children.Add(_activeGround[i]);
                Container.Children.Add(_activeLabels[i]);
            }

            _ground = new MapLayer(Container, _activeGround);
            _labels = new MapLayer(Container, _activeLabels);
            _brush = new CurrentBrush();

            var cursor = new LabeledCursor(Container, currentBrush: _brush, _coordinates);

            _palette = new Palette(_brush, _coordinates);
            Container.Children.Add(_palette);

            _interactions = new Interactions(
                this,
                Container,

                new Interaction(
                    new Blocking(
                        new WhenRightMouseButtonClicked()
                    ),
                    new WithPredefinedCursor(
                        Container,
                        cursor,
                        Cursors.Arrow,
                        new Show(
                            _palette,
                            Container
                        )
                    )
                ),

                new Interaction(
                    new AlwaysOn(),
                    new MoveCustomCursor(
                        Container,
                        cursor
                    )
                ),

                new Interaction(
                    new Blocking(
                        new WhenKeyHold(Key.Space)
                    ),
                    new WithPredefinedCursor(
                        Container,
                        cursor,
                        Cursors.Hand,
                        new MoveAndScale(
                            Container,
                            _coordinates,
                            this
                        )
                    )
                ),

                new Interaction(
                    new Blocking(
                        new WhenKeyPressed(Key.X)
                    ),
                    new WithStatus(
                        "Draw on labels",
                        this,
                        new WithCustomCursor(
                            Container,
                            "labels",
                            cursor,
                            new Draw(
                                _labels,
                                _brush
                            )
                        )
                    )
                ),

                new Interaction(
                    new Blocking(
                        new WhenKeyPressed(Key.Z)
                    ),
                    new WithStatus(
                        "Draw on ground",
                        this,
                        new WithCustomCursor(
                            Container,
                            "ground",
                            cursor,
                            new Draw(
                                _ground,
                                _brush
                            )
                        )
                    )
                ),

                new Interaction(
                    new Blocking(
                        new WhenKeysPressed(
                            on: Key.LeftCtrl,
                            off: Key.LeftShift
                        )
                    ),

                    new WithStatus(
                        "Measuring",
                        this,
                        new WithPredefinedCursor(
                            Container,
                            cursor,
                            Cursors.Pen,
                            new Measure(
                                this,
                                Container,
                                _measure
                            )
                        )
                    )

                ),

                new Interaction(
                    new AlwaysOn(),
                    new WithStatus(
                        "Exploring",
                        this,
                        new WithPredefinedCursor(
                            Container,
                            cursor,
                            Cursors.Hand,
                            new MoveAndScale(
                                Container,
                                _coordinates,
                                this
                            )
                        )
                    )
                )
            );

            _coordinates.Reset();

            _mapProvider.LoadMap("1");

            _coordinates.ActualWidthInMeters = _mapProvider.Settings.Width;

            Reposition(1, new Point(0, 0), new Vector(0, 0));
        }

        public void Reposition(double lastScale, Point zoomLocation, Vector offset)
        {
            _coordinates.MoveAndScaleFrame(
                lastScale,
                scaleCenter: new PointDouble2D()
                {
                    X = zoomLocation.X,
                    Y = zoomLocation.Y
                },
                offset: new PointDouble2D()
                {
                    X = -offset.X,
                    Y = -offset.Y
                }
            );

            var level0 = _coordinates.GetVisibleItems(_coordinates.DescreetLevel);
            var level1 = _coordinates.GetVisibleItems(_coordinates.DescreetLevel + 1);

            PositionLevel(level0, 0);
            PositionLevel(level1, 1);
        }

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

        private void LoadGround(MapPart uiImageView, ImagePosition index2D)
        {
            uiImageView.Load(index2D);
        }

        private void LoadLabel(MapPart uiImageView, ImagePosition index2D)
        {
            uiImageView.Load(index2D);
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

        public void Set(string text)
        {
            Title = text;
        }
    }
}
