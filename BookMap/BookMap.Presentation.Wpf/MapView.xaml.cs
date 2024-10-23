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
using BookMap.Presentation.Wpf.Core;
using BookMap.Presentation.Wpf.InteractionModels;
using BookMap.Presentation.Wpf.MapModels;
using BookMap.Presentation.Wpf.MapModels.DrawModels;
using BookMap.Presentation.Wpf.Models;
using BookMap.Presentation.Wpf.Views;
using BookWiki.Core.Utils;
using Newtonsoft.Json;
using Measure = BookMap.Presentation.Wpf.InteractionModels.Measure;
using Path = System.IO.Path;

namespace BookMap.Presentation.Wpf
{
    //todo:
    // useful api to draw on image
    // features:
    //  markers
    //   layer for markers display
    //   api to extend and display markers on map with links to wiki
    //  textures
    //   drawing with textures
    //   import textures
    public partial class MapView : UserControl, IMapView, ILabel
    {
        private IPinsFeature _pinsFeature;
        private readonly MapProviderSynchronous _mapProvider;
        private readonly CoordinateSystem _coordinates;
        private readonly Models.Measure _measure;

        private MapPart[] _activeGround = new MapPart[9];
        private MapPart[] _nextGround = new MapPart[9];
        private MapPart[] _activeLabels = new MapPart[9];
        private MapPart[] _nextLabels = new MapPart[9];

        private MapLayer _ground;

        public bool ShowLabels = true;
        private AppConfigDto? _config;

        private MapLayer _labels;
        private Interactions _interactions;
        private CurrentBrush _brush;
        private PinsLayer _pins;
        private Palettes _palettes;

        public MapView()
        {
            _coordinates = new CoordinateSystem();
            _mapProvider = new MapProviderSynchronous(new WpfImageFactory());
            _measure = new Models.Measure(_coordinates);

            Bookmarks = new Bookmarks(_mapProvider, this);

            InitializeComponent();
        }

        public void InitPins(IPinsFeature pinsFeature)
        {
            _pinsFeature = pinsFeature;
        }

        public Bookmarks Bookmarks { get; }

        public PinsLayer Pins => _pins;

        public void Init(Window window, string path)
        {
            Init(
                window,
                Path.GetFileName(path),
                new AppConfigDto()
                {
                    Root = path.Replace(Path.GetFileName(path), "")
                }
            );
        }

        public void CleanUp()
        {
            _mapProvider.ClearCache();
        }

        public void Init(Window window, string mapName, AppConfigDto config = null)
        {
            _pinsFeature ??= new PinsAsLabelFeature();

            _pins = new PinsLayer(_coordinates, _pinsFeature, _mapProvider);

            _config = config ?? JsonConvert.DeserializeObject<AppConfigDto>(File.ReadAllText("MapConfig.json"));

            FileSystemServiceSynchronous.DirectoryPath = _config.Root;

            var mapInfo = new MapReference(_config, mapName);

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

            Container.Children.Add(_pins);

            _ground = new MapLayer(Container, _activeGround);
            _labels = new MapLayer(Container, _activeLabels);

            _brush = new CurrentBrush();

            //var drawingCursor = new LabeledCursor(Container, currentBrush: _brush, _coordinates);
            //var textCursor = new LabeledCursor(Container, currentBrush: _brush, _coordinates, new IBeam());
            var cursors = new List<LabeledCursor>();

            var toolKeys = new List<Key>();

            _palettes = new Palettes(Container);

            _interactions = new Interactions(
                window,
                Container
            );

            _interactions.Add(
                new Interaction(
                    new Blocking(
                        new WhenRightMouseButtonClicked()
                    ),
                    new WithPredefinedCursor(
                        Container,
                        cursors,
                        Cursors.Arrow,
                        new TryShowActivePalette(
                            _palettes,
                            Container
                        )
                    )
                ),

                new Interaction(
                    new AlwaysOn(),
                    new MoveCustomCursors(
                        Container,
                        cursors
                    )
                ),

                //new Interaction(
                //    new AlwaysOn(),
                //    new MoveCustomCursor(
                //        Container,
                //        drawingCursor
                //    )
                //),

                //new Interaction(
                //    new AlwaysOn(),
                //    new MoveCustomCursor(
                //        Container,
                //        textCursor
                //    )
                //),

                new Interaction(
                    new Blocking(
                        new WhenKeyHold(Key.Space)
                    ),
                    new WithPredefinedCursor(
                        Container,
                        cursors,
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
                        new WhenKeyPressed(Key.X.AddInto(toolKeys), deactivation: toolKeys)
                    ),
                    new WithPalette(
                        "Draw on labels".As(out var drawOnLabelsFeature),
                        _palettes,
                        new Palette(new CurrentBrush().As(out var labelBrush), _coordinates, new BrushesFromMapSettingsPalettes(drawOnLabelsFeature, _mapProvider)),
                        new WithStatus(
                            drawOnLabelsFeature,
                            this,
                            new WithCustomCursor(
                                Container,
                                "labels",
                                new LabeledCursor(Container, currentBrush: labelBrush, _coordinates).AddInto(cursors),
                                cursors,
                                new Draw(
                                    _labels,
                                    labelBrush
                                )
                            )
                        )
                        )
                    
                ),

                new Interaction(
                    new Blocking(
                        new WhenKeyPressed(Key.Z.AddInto(toolKeys), deactivation: toolKeys)
                    ),
                    new WithPalette(
                        "Draw on ground".As(out var drawOnGroundFeature),
                        _palettes,
                        new Palette(new CurrentBrush().As(out var groundBrush), _coordinates, new BrushesFromMapSettingsPalettes(drawOnGroundFeature, _mapProvider)),
                        new WithStatus(
                            drawOnGroundFeature,
                            this,
                            new WithCustomCursor(
                                Container,
                                "ground",
                                new LabeledCursor(Container, currentBrush: groundBrush, _coordinates).AddInto(cursors),
                                cursors,
                                new Draw(
                                    _ground,
                                    groundBrush
                                )
                            )
                        )
                        )
                ),

                new Interaction(
                    new Locked(
                        new WhenKeyPressed(Key.C.AddInto(toolKeys), deactivation: toolKeys)
                    ).As(out var renderTextInteractionLock),
                    new WithPalette(
                        "Write text".As(out var writeTextFeature),
                        _palettes,
                        new Palette(new CurrentBrush().As(out var textBrush), _coordinates, new BrushesFromMapSettingsPalettes(writeTextFeature, _mapProvider)),
                        new WithStatus(
                            writeTextFeature,
                            this,
                            new WithCustomCursor(
                                Container,
                                "text",
                                new LabeledCursor(Container, currentBrush: textBrush, _coordinates, new IBeam()).As(out var customBeamCursor).AddInto(cursors),
                                cursors,
                                new RenderText(
                                    Container,
                                    _labels,
                                    textBrush,
                                    renderTextInteractionLock,
                                    customBeamCursor,
                                    _interactions
                                )
                            )
                        )
                        )
                ),

                new Interaction(
                    new Blocking(
                        new WhenKeyPressed(Key.V.AddInto(toolKeys), deactivation: toolKeys)
                    ),
                    new WithStatus(
                        "Add pin",
                        this,
                        new WithPredefinedCursor(
                            Container,
                            cursors,
                            Cursors.Help,
                            new AddPin(
                                Container,
                                _coordinates,
                                this,
                                _pins
                            )
                        )
                    )
                ),

                new Interaction(
                    new Blocking(
                        new WhenKeysPressed(
                            on: Key.LeftCtrl.AddInto(toolKeys),
                            off: Key.LeftShift.AddInto(toolKeys)
                        )
                    ),

                    new WithStatus(
                        "Measuring",
                        this,
                        new WithPredefinedCursor(
                            Container,
                            cursors,
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
                            cursors,
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

            _mapProvider.LoadMap(mapName);

            _pins.Load();

            _coordinates.ActualWidthInMeters = _mapProvider.Settings.Width;

            Reposition(1, new Point(0, 0), new Vector(0, 0));
        }

        public void PositionMapToBookmark(BookmarkV2 bookmark)
        {
            _coordinates.Position(bookmark.World);

            Reposition(1, new Point(0, 0), new Vector(0, 0));
        }

        public void PositionMapToRegion(FrameDouble region)
        {
            _coordinates.Position(region);

            Reposition(1, new Point(0, 0), new Vector(0, 0));
        }

        public BookmarkV2 ExtractBookmark()
        {
            return new BookmarkV2()
            {
                Id = Guid.NewGuid(),
                Name = string.Empty,
                World = _coordinates.CurrentWorld.Clone()
            };
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

            Pins.Reposition();
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
            Info.Text = text;
        }
    }
}
