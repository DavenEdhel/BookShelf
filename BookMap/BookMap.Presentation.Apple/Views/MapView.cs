using System;
using System.Drawing;
using System.Linq;
using BookMap.Presentation.Apple.Extentions;
using BookMap.Presentation.Apple.Models;
using BookMap.Presentation.Apple.Services;
using CoreGraphics;
using Foundation;
using UIKit;

namespace BookMap.Presentation.Apple.Views
{
    public class MapView : UIView
    {
        private UIImageView[] _activeGround = new UIImageView[9];
        private UIImageView[] _nextGround = new UIImageView[9];
        private UIImageView[] _activeLabels = new UIImageView[9];
        private UIImageView[] _nextLabels = new UIImageView[9];
        private UIImageView[] _measures = new UIImageView[9];

        private CGPoint _additionalOffset = new CGPoint(0, 0);
        private nfloat _lastScale = 1;

        private CGPoint _zoomLocation;

        private bool _moveInProgress = false;
        private bool _zoomInProgress = false;
        private TouchMode _touchMode = TouchMode.MoveAndDraw;

        private readonly CoordinateSystem _coordinates;
        private readonly PaletteView _paletteView;

        private readonly MapProvider _mapProvider;
        private readonly ActionBarView _topPanel;

        public event Action<UIColor> ColorSelected = delegate { };

        public MapView(CoordinateSystem coordinateSystem, PaletteView paletteView, MapProvider mapProvider, ActionBarView topPanel)
        {
            Initialize();

            _coordinates = coordinateSystem;
            _paletteView = paletteView;
            _mapProvider = mapProvider;
            _topPanel = topPanel;
        }

        private void Initialize()
        {
            BackgroundColor = "#E0E0E0".ToUIColor();

            for (int i = 0; i < _nextGround.Length; i++)
            {
                _nextGround[i] = new UIImageView();
                _activeGround[i] = new UIImageView();
                _nextLabels[i] = new UIImageView();
                _activeLabels[i] = new UIImageView();
                _measures[i] = new UIImageView();
                _measures[i].Image = ImageHelper.MakeEmptyImage();
                Add(_nextGround[i]);
                Add(_nextLabels[i]);
                Add(_activeGround[i]);
                Add(_activeLabels[i]);
                Add(_measures[i]);
            }

            _rangeLabel = new UILabel();
            _rangeLabel.Font = UIFont.BoldSystemFontOfSize(20);
            _rangeLabel.TextColor = UIColor.Black;
            _rangeLabel.BackgroundColor = UIColor.White.ColorWithAlpha(0.3f);

            _distanceLabel = new UILabel();
            _distanceLabel.Font = UIFont.BoldSystemFontOfSize(20);
            _distanceLabel.TextColor = UIColor.Black;
            _distanceLabel.BackgroundColor = UIColor.White.ColorWithAlpha(0.3f);

            _clearMeasures = new UIButton();
            _clearMeasures.BackgroundColor = UIColor.White;
            _clearMeasures.SetTitle("Clear Measures", UIControlState.Normal);
            _clearMeasures.SetTitleColor(UIColor.Red, UIControlState.Normal);
            _clearMeasures.TouchUpInside += ClearMeasuresOnTouchUpInside;

            _applyDrawUp = new UIButton();
            _applyDrawUp.BackgroundColor = UIColor.White;
            _applyDrawUp.SetTitle("Apply draw up", UIControlState.Normal);
            _applyDrawUp.SetTitleColor(UIColor.Red, UIControlState.Normal);
            _applyDrawUp.TouchUpInside += ApplyDrawUp;

            _cancelDrawUp = new UIButton();
            _cancelDrawUp.BackgroundColor = UIColor.White;
            _cancelDrawUp.SetTitle("Cancel draw up", UIControlState.Normal);
            _cancelDrawUp.SetTitleColor(UIColor.Red, UIControlState.Normal);
            _cancelDrawUp.TouchUpInside += CancelDrawUp;

            //_nextGround[0].BackgroundColor = UIColor.Red;
            //_nextGround[1].BackgroundColor = UIColor.Blue;
            //_nextGround[2].BackgroundColor = UIColor.Green;
            //_nextGround[3].BackgroundColor = UIColor.Yellow;
            //_nextGround[4].BackgroundColor = UIColor.DarkGray;
            //_nextGround[5].BackgroundColor = UIColor.Brown;
            //_nextGround[6].BackgroundColor = UIColor.Orange;
            //_nextGround[7].BackgroundColor = UIColor.LightGray;
            //_nextGround[8].BackgroundColor = UIColor.White;

            //_activeGround[0].BackgroundColor = UIColor.Red;
            //_activeGround[1].BackgroundColor = UIColor.Blue;
            //_activeGround[2].BackgroundColor = UIColor.Green;
            //_activeGround[3].BackgroundColor = UIColor.Yellow;
            //_activeGround[4].BackgroundColor = UIColor.DarkGray;
            //_activeGround[5].BackgroundColor = UIColor.Brown;
            //_activeGround[6].BackgroundColor = UIColor.Orange;
            //_activeGround[7].BackgroundColor = UIColor.LightGray;
            //_activeGround[8].BackgroundColor = UIColor.White;

            foreach (var uiImageView in _nextGround)
            {
                this.BringSubviewToFront(uiImageView);
            }

            _zoom = new UIPinchGestureRecognizer(OnZoom)
            {
                AllowedTouchTypes = new NSNumber[] { 0 },
                ShouldRecognizeSimultaneously = ShouldRecognizeSimultaneously,
                Enabled = TouchMode == TouchMode.MoveAndDraw
            };

            _move = new UIPanGestureRecognizer(OnMove)
            {
                AllowedTouchTypes = new NSNumber[] { 0 },
                ShouldRecognizeSimultaneously = ShouldRecognizeSimultaneously,
                Enabled = TouchMode == TouchMode.MoveAndDraw
            };

            AddGestureRecognizer(_zoom);
            AddGestureRecognizer(_move);
        }

        private void ClearMeasuresOnTouchUpInside(object sender, EventArgs eventArgs)
        {
            foreach (var uiImageView in _measures)
            {
                var old = uiImageView.Image;
                uiImageView.Image = ImageHelper.MakeEmptyImage();
                old.Dispose();
            }

            _clearMeasures.RemoveFromSuperview();

            _topPanel.StopMeasuring();
        }

        private UIPinchGestureRecognizer _zoom;
        private UIPanGestureRecognizer _move;

        public TouchMode TouchMode
        {
            get { return _touchMode; }
            set
            {
                if (_touchMode == TouchMode.MoveAndMeasure && _touchMode != value)
                {
                    _rangeLabel.RemoveFromSuperview();
                    _distanceLabel.RemoveFromSuperview();
                    _clearMeasures.RemoveFromSuperview();
                }

                if (_touchMode != TouchMode.MoveAndMeasure && value == TouchMode.MoveAndMeasure)
                {
                    _rangeLabel.Frame = new CGRect(new CGPoint(Frame.Width / 2, Frame.Top + 100), new CGSize(300, 50));
                    _distanceLabel.Frame = new CGRect(new CGPoint(Frame.Width / 2, _rangeLabel.Frame.Bottom), new CGSize(300, 50));
                    _clearMeasures.Frame = new CGRect(new CGPoint(Frame.Width / 2, _distanceLabel.Frame.Bottom), new CGSize(300, 50));
                    _rangeLabel.Text = "Range: 0 M";
                    _distanceLabel.Text = "Distance: 0 M";
                    _mapProvider.FlushToFileSystem();
                    Add(_clearMeasures);
                    Add(_distanceLabel);
                    Add(_rangeLabel);
                }

                _touchMode = value;
                _zoom.Enabled = _touchMode == TouchMode.MoveAndDraw || _touchMode == TouchMode.MoveAndMeasure;
                _move.Enabled = _touchMode == TouchMode.MoveAndDraw || _touchMode == TouchMode.MoveAndMeasure;

                System.Diagnostics.Debug.WriteLine(_touchMode);
            }
        }

        public DrawMode DrawMode { get; set; }

        public bool ShowLabels
        {
            get { return _showLabels; }
            set
            {
                _showLabels = value;

                PositionMap();
            }
        }

        public bool ColorEditMode { get; set; }

        private CGContext _context;
        private CGPoint _first;
        private UIImageView _active;
        private bool _showLabels;
        private double _distance;
        private double _range;
        private UILabel _rangeLabel;

        private CGPoint _veryFirst;
        private UILabel _distanceLabel;
        private UIButton _clearMeasures;
        private UIButton _cancelDrawUp;

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            base.TouchesBegan(touches, evt);

            var touch = (UITouch)touches.AnyObject;

            if (ColorEditMode && touch.Type == UITouchType.Stylus)
            {  
                SelectColor(touch);

                return;
            }

            if (touch.Type != UITouchType.Stylus && TouchMode != TouchMode.DrawOnly)
            {
                return;
            }

            var check = touch.LocationInView(this);

            _active = GetActiveView(check);

            UIGraphics.BeginImageContext(_active.Image.Size);

            _first = touch.LocationInView(_active);

            _veryFirst = new CGPoint(_first.X, _first.Y);

            _context = UIGraphics.GetCurrentContext();

            _context.TranslateCTM(0f, _active.Image.Size.Height);
            _context.ScaleCTM(1.0f, -1.0f);
            _context.DrawImage(new RectangleF(0f, 0f, _active.Image.CGImage.Width, _active.Image.CGImage.Height), _active.Image.CGImage);
            _context.ScaleCTM(1.0f, -1.0f);
            _context.TranslateCTM(0f, -_active.Image.CGImage.Height);
            _context.SetLineCap(CGLineCap.Round);
            SetupBrush();

            if (_touchMode == TouchMode.MoveAndMeasure)
            {
                _distance = 0;
                _range = 0;
            }
        }

        private void SetupBrush()
        {
            if (_touchMode == TouchMode.MoveAndMeasure)
            {
                _context.SetLineWidth(2.5f);
                _context.SetStrokeColor(UIColor.Red.CGColor);

                return;
            }
            
            _context.SetLineWidth(_paletteView.CurrentBrush.Size * 2.5f);
            if (_paletteView.CurrentBrush.IsEraser)
            {
                _context.SetBlendMode(CGBlendMode.Clear);
            }
            _context.SetStrokeColor(_paletteView.CurrentBrush.Color.CGColor);
        }

        private UIImageView GetActiveView(CGPoint check)
        {
            if (TouchMode == TouchMode.MoveAndMeasure)
            {
                return _measures.First(x => x.Frame.Contains(check));
            }

            if (DrawMode == DrawMode.Ground)
            {
                return _activeGround.First(x => x.Frame.Contains(check));
            }
            else
            {
                return _activeLabels.First(x => x.Frame.Contains(check));
            }
        }

        public override void TouchesMoved(NSSet touches, UIEvent evt)
        {
            base.TouchesMoved(touches, evt);

            var touch = (UITouch)touches.AnyObject;

            if (ColorEditMode && touch.Type == UITouchType.Stylus)
            {
                SelectColor(touch);

                return;
            }

            if (touch.Type != UITouchType.Stylus && TouchMode != TouchMode.DrawOnly)
            {
                return;
            }

            var next = touch.LocationInView(_active);

            var path = new CGPath();
            path.MoveToPoint(_first);

            var scale = _active.Frame.Width / Frame.Width;
            var ds = 2.5f / scale;

            var first = new CGPoint(_first.X * ds, _first.Y * ds);
            var second = new CGPoint(next.X * ds, next.Y * ds);

            if (TouchMode == TouchMode.MoveAndMeasure)
            {
                _distance += _coordinates.GetDistance(_first.ToPoint(), next.ToPoint());
                _distanceLabel.Text = $"Distance: {_distance:F1} M";

                _range = _coordinates.GetDistance(_veryFirst.ToPoint(), next.ToPoint());
                _rangeLabel.Text = $"Range: {_range:F1} M";
            }

            path.AddLines(new CGPoint[] {first, second});

            _context.AddPath(path);
            _context.DrawPath(CGPathDrawingMode.Stroke);

            var newImage = UIGraphics.GetImageFromCurrentImageContext();

            if (_touchMode != TouchMode.MoveAndMeasure)
            {
                _mapProvider.Modify(_active.Image.ToImage(), newImage.ToImage(), andSaveToFileSystem: _touchMode != TouchMode.MoveAndMeasure);
                _active.Image = newImage;
            }
            else
            {
                _active.Image.Dispose();
                _active.Image = newImage;
            }

            path.Dispose();
            path = null;

            _active.SetNeedsLayout();

            var check = touch.LocationInView(this);

            var newActive = GetActiveView(check);

            if (newActive != _active)
            {
                _context?.Dispose();
                _context = null;

                UIGraphics.EndImageContext();

                if (next.X > _active.Frame.Width)
                {
                    _first.X -= _active.Frame.Width;
                }

                if (next.X < 0)
                {
                    _first.X += _active.Frame.Width;
                }

                if (next.Y > _active.Frame.Height)
                {
                    _first.Y -= _active.Frame.Height;
                }

                if (next.Y < 0)
                {
                    _first.Y += _active.Frame.Height;
                }

                _active = newActive;

                UIGraphics.BeginImageContext(_active.Image.Size);

                next = touch.LocationInView(_active);

                _context = UIGraphics.GetCurrentContext();

                _context.TranslateCTM(0f, _active.Image.Size.Height);
                _context.ScaleCTM(1.0f, -1.0f);
                _context.DrawImage(new RectangleF(0f, 0f, _active.Image.CGImage.Width, _active.Image.CGImage.Height), _active.Image.CGImage);
                _context.ScaleCTM(1.0f, -1.0f);
                _context.TranslateCTM(0f, -_active.Image.CGImage.Height);
                _context.SetLineCap(CGLineCap.Round);
                SetupBrush();

                var newPath = new CGPath();
                newPath.MoveToPoint(_first);

                var newScale = _active.Frame.Width / Frame.Width;
                var newDs = 2.5f / newScale;

                //_first = new CGPoint(_first.X%_active.Frame.Width, _first.Y%_active.Frame.Height);

                newPath.AddLines(new CGPoint[] { new CGPoint(_first.X * newDs, _first.Y * newDs), new CGPoint(next.X * newDs, next.Y * newDs) });

                _context.AddPath(newPath);
                _context.DrawPath(CGPathDrawingMode.Stroke);

                newImage = UIGraphics.GetImageFromCurrentImageContext();

                if (_touchMode != TouchMode.MoveAndMeasure)
                {
                    _mapProvider.Modify(_active.Image.ToImage(), newImage.ToImage(), andSaveToFileSystem: _touchMode != TouchMode.MoveAndMeasure);
                    _active.Image = newImage;
                }
                else
                {
                    _active.Image.Dispose();
                    _active.Image = newImage;
                }

                newPath.Dispose();
                newPath = null;

                _active.SetNeedsLayout();
            }

            _first = next;
        }

        private void SelectColor(UITouch touch)
        {
            var check = touch.LocationInView(this);

            var activeView = GetActiveView(check);

            var pointInView = touch.LocationInView(activeView);

            var scale = activeView.Frame.Width / Frame.Width;
            var ds = 2.5f / scale;

            var location = new CGPoint(pointInView.X * ds, pointInView.Y * ds);

            var color = GetPixelColor(activeView.Image, (int)location.X, (int)location.Y, (int)activeView.Image.CGImage.Width);

            ColorSelected(color);
        }

        private UIColor GetPixelColor(UIImage image, int x, int y, int width)
        {
            var pixelData = image.CGImage.DataProvider.CopyData();
            var pixelLocation = ((width * y) + x) * 4;

            var r = (pixelData[pixelLocation]) / (255.0f);
            var g = (pixelData[pixelLocation + 1]) / (255.0f);
            var b = (pixelData[pixelLocation + 2]) / (255.0f);
            var a = (pixelData[pixelLocation + 3]) / (255.0f);

            if (image.CGImage.BitmapInfo.HasFlag(CGBitmapFlags.ByteOrder32Little))
            {
                return UIColor.FromRGBA(b, g, r, a);
            }

            return UIColor.FromRGBA(r, g, b, a);
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(touches, evt);

            var touch = (UITouch)touches.AnyObject;

            if (ColorEditMode && touch.Type == UITouchType.Stylus)
            {
                return;
            }

            if (touch.Type != UITouchType.Stylus && TouchMode != TouchMode.DrawOnly)
            {
                _context?.Dispose();
                _context = null;

                return;
            }

            var next = touch.LocationInView(_active);

            var path = new CGPath();
            path.MoveToPoint(_first);

            var scale = _active.Frame.Width / Frame.Width;
            var ds = 2.5f / scale;

            path.AddLines(new CGPoint[] { new CGPoint(_first.X * ds, _first.Y * ds), new CGPoint(next.X * ds, next.Y * ds) });

            if (TouchMode == TouchMode.MoveAndMeasure)
            {
                _distance += _coordinates.GetDistance(_first.ToPoint(), next.ToPoint());
                _distanceLabel.Text = $"Distance: {_distance:F1} M";

                _range = _coordinates.GetDistance(_veryFirst.ToPoint(), next.ToPoint());
                _rangeLabel.Text = $"Range: {_range:F1} M";
            }

            _context.AddPath(path);
            _context.DrawPath(CGPathDrawingMode.Stroke);

            var newImage = UIGraphics.GetImageFromCurrentImageContext();

            if (_touchMode != TouchMode.MoveAndMeasure)
            {
                _mapProvider.Modify(_active.Image.ToImage(), newImage.ToImage(), andSaveToFileSystem: _touchMode != TouchMode.MoveAndMeasure);
                _active.Image = newImage;
            }
            else
            {
                _active.Image.Dispose();
                _active.Image = newImage;
            }

            path.Dispose();
            path = null;

            _active.SetNeedsLayout();

            _context?.Dispose();
            _context = null;

            UIGraphics.EndImageContext();
        }

        private bool ShouldRecognizeSimultaneously(UIGestureRecognizer gestureRecognizer, UIGestureRecognizer otherGestureRecognizer)
        {
            return true;
        }

        private void OnMove(UIPanGestureRecognizer parameters)
        {
            if (parameters.State == UIGestureRecognizerState.Began)
            {
                _coordinates.Begin();

                _moveInProgress = true;
            }
            else if (parameters.State == UIGestureRecognizerState.Changed)
            {
                _additionalOffset = parameters.TranslationInView(this);

                //System.Diagnostics.Debug.WriteLine($"{_additionalOffset.X}; {_additionalOffset.Y};");

                if (_zoomInProgress == false)
                {
                    PositionMap();
                }
            }
            else if (parameters.State == UIGestureRecognizerState.Ended)
            {
                _coordinates.End();

                _additionalOffset = new CGPoint(0, 0);

                _moveInProgress = false;
            }
        }

        private void OnZoom(UIPinchGestureRecognizer parameters)
        {
            if (parameters.State == UIGestureRecognizerState.Began)
            {
                _coordinates.Begin();

                _zoomInProgress = true;

                _zoomLocation = parameters.LocationInView(this);
            }
            else if (parameters.State == UIGestureRecognizerState.Changed)
            {
                _lastScale = parameters.Scale;

                PositionMap();
            }
            else if (parameters.State == UIGestureRecognizerState.Ended)
            {
                _coordinates.End();

                _lastScale = 1;

                _zoomInProgress = false;
            }
        }

        public void PositionMapToBookmark(Bookmark bookmark)
        {
            _coordinates.Position(bookmark);

            PositionMap();
        }

        public Bookmark ExtractBookmark()
        {
            return new Bookmark()
            {
                Name = string.Empty,
                World = _coordinates.CurrentWorld.Clone()
            };
        }

        public void PositionMap()
        {
            _coordinates.MoveAndScaleFrame(_lastScale, _zoomLocation.ToPoint(), _additionalOffset.ToPoint());

            var level0 = _coordinates.GetVisibleItems(_coordinates.DescreetLevel);
            var level1 = _coordinates.GetVisibleItems(_coordinates.DescreetLevel + 1);

            PositionLevel(level0, 0);
            PositionLevel(level1, 1);
            PositionMeasures(level0);
        }

        private void PositionMeasures(ImagePosition[] positions)
        {
            var worldPositions = _coordinates.GetWorldPositions(positions);

            for (int i = 0; i < positions.Length; i++)
            {
                UIImageView part = _measures[i];
                FrameDouble frame = worldPositions[i];

                if (frame == null)
                {
                    continue;
                }

                part.Frame = new CGRect(frame.X, frame.Y, frame.Width, frame.Height);
            }
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
                    label[i].Image = null;
                }
            }
        }

        private async void LoadGround(UIImageView uiImageView, ImagePosition index2D)
        {
            uiImageView.Image = (await _mapProvider.GetImageAsync(index2D, isLabel: false)).ToUIImage();
        }

        private async void LoadLabel(UIImageView uiImageView, ImagePosition index2D)
        {
            uiImageView.Image = (await _mapProvider.GetImageAsync(index2D, isLabel: true)).ToUIImage();
        }

        private void SetPosition(UIImageView part, FrameDouble frame, ImagePosition imagePosition)
        {
            if (frame == null)
            {
                part.Alpha = 0;
                return;
            }

            part.Frame = new CGRect(frame.X, frame.Y, frame.Width, frame.Height);

            var a = imagePosition.Level - _coordinates.Level;
            var isUpperLevel = a < 0;

            var da = 0.05f;

            var alpha = (isUpperLevel ? (nfloat)((1 + a) / da) : (nfloat)(1 - a / da));

            part.Alpha = alpha;
        }

        private void SetSubImage(UIImageView ground, UIImage image, FrameDouble imageFrame)
        {
            if (ground.Image == null)
            {
                return;
            }

            //define parameters to manipulate with
            double? ldx = CalculateLeading(imageFrame.X, ground.Frame.X);
            double? ldy = CalculateLeading(imageFrame.Y, ground.Frame.Y);
            double? tdx = CalculateTrailing(imageFrame.X, imageFrame.Width, ground.Frame.X, ground.Frame.Width);
            double? tdy = CalculateTrailing(imageFrame.Y, imageFrame.Height, ground.Frame.Y, ground.Frame.Height);
            double? dx = CalculateInner(imageFrame.X, ground.Frame.X);
            double? dy = CalculateInner(imageFrame.Y, ground.Frame.Y);
            var affineScaling = ground.Frame.Width / ground.Image.Size.Width;

            var width = imageFrame.Width / affineScaling;
            var height = imageFrame.Height / affineScaling;

            var adx = CalculateAffineOffset(ldx, dx, affineScaling);
            var ady = ground.Image.CGImage.Height - CalculateAffineOffset(ldy, dy, affineScaling) - height;

            var scaledImage = image.Scale(new CGSize(width, height));

            UIGraphics.BeginImageContext(ground.Image.Size);

            var context = UIGraphics.GetCurrentContext();

            context.TranslateCTM(0f, (nfloat)ground.Image.Size.Height);
            context.ScaleCTM(1.0f, -1.0f);

            context.DrawImage(new RectangleF(0f, 0f, ground.Image.CGImage.Width, ground.Image.CGImage.Height), ground.Image.CGImage);
            context.DrawImage(new CGRect(adx, ady, width, height), scaledImage.CGImage);

            context.ScaleCTM(1.0f, -1.0f);
            context.TranslateCTM(0f, -(nfloat)ground.Image.Size.Height);

            var raw = UIGraphics.GetImageFromCurrentImageContext();

            scaledImage.Dispose();

            context.Dispose();

            UIGraphics.EndImageContext();

            _mapProvider.Modify(ground.Image.ToImage(), raw.ToImage());

            ground.Image = raw;
        }

        private double CalculateAffineOffset(double? ld, double? d, double affineScale)
        {
            if (ld != null)
            {
                return -ld.Value / affineScale;
            }

            if (d != null)
            {
                return d.Value / affineScale;
            }

            return 0;
        }

        private double? CalculateLeading(double imageC, double groundC)
        {
            if (imageC < groundC) // image is behind the ground
            {
                return groundC - imageC;
            }

            return null;
        }

        private double? CalculateInner(double imageC, double groundC)
        {
            if (imageC > groundC)
            {
                return imageC - groundC;
            }

            return null;
        }

        private double? CalculateTrailing(double imageC, double imageS, double groundC, double groundS)
        {
            var imageT = imageC + imageS;
            var groundT = groundC + groundS;

            if (imageT > groundT)
            {
                return imageT - groundT;
            }

            return null;
        }

        public void Reset()
        {
            PositionMap();
        }

        public async void Load(string mapName, string bookmark)
        {
            _coordinates.Reset();

            await _mapProvider.LoadMap(mapName);

            _coordinates.ActualWidthInMeters = _mapProvider.Settings.Width;

            PositionMap();

            if (string.IsNullOrWhiteSpace(bookmark) == false)
            {
                var b = _mapProvider.Settings.Bookmarks.FirstOrDefault(x =>
                    string.Equals(x.Name, bookmark, StringComparison.InvariantCultureIgnoreCase));

                if (b != null)
                {
                    PositionMapToBookmark(b);
                }
            }
        }

        public async void Load(string mapName)
        {
            _coordinates.Reset();

            await _mapProvider.LoadMap(mapName);

            _coordinates.ActualWidthInMeters = _mapProvider.Settings.Width;

            PositionMap();
        }

        public void ImportImage(FrameDouble frame, UIImage import)
        {
            var positions = _coordinates.GetVisibleItems(_coordinates.DescreetLevel);

            for (int i = 0; i < positions.Length; i++)
            {
                SetSubImage(_activeGround[i], import, frame);
            }
        }

        private ImagePosition[] _visibleItems;
        private UIButton _applyDrawUp;

        public void DrawUp()
        {
            _cancelDrawUp.Frame = new CGRect(Frame.Width/2, 50, 200, 50);
            Add(_cancelDrawUp);

            _applyDrawUp.Frame = new CGRect(Frame.Width / 2, _cancelDrawUp.Frame.Bottom, 200, 50);
            Add(_applyDrawUp);

            _mapProvider.FlushToFileSystem();

            _visibleItems = _coordinates.GetVisibleItems(_coordinates.DescreetLevel).Where(x => x != null).ToArray();

            foreach (var imagePosition in _visibleItems)
            {
                System.Diagnostics.Debug.WriteLine(imagePosition);
            }

            _mapProvider.RedrawAnew(_visibleItems).ContinueWith(task =>
            {
                InvokeOnMainThread(PositionMap);
            });
        }

        private void CancelDrawUp(object sender, EventArgs e)
        {
            _mapProvider.CancelDrawUp(_visibleItems);

            PositionMap();

            _cancelDrawUp.RemoveFromSuperview();
            _applyDrawUp.RemoveFromSuperview();
        }

        private void ApplyDrawUp(object sender, EventArgs e)
        {
            _mapProvider.ApplyDrawUp(_visibleItems);

            PositionMap();

            _cancelDrawUp.RemoveFromSuperview();
            _applyDrawUp.RemoveFromSuperview();
        }
    }
}