using System;
using BookMap.Presentation.Apple.Services;

namespace BookMap.Presentation.Apple.Models
{
    public class CoordinateSystem
    {
        private FrameDouble _currentWorld = new FrameDouble()
        {
            X = 0,
            Y = 0,
            Width = 1536,
            Height = 1152
        };

        private FrameDouble _lastWorld = new FrameDouble()
        {
            X = 0,
            Y = 0,
            Width = 1536,
            Height = 1152
        };

        private readonly FrameDouble _originalWorld = new FrameDouble()
        {
            X = 0,
            Y = 0,
            Width = 1536,
            Height = 1152
        };

        public double ActualWidthInMeters { get; set; } = 100000;

        public FrameDouble CurrentWorld => _currentWorld;

        public double Scale => _currentWorld.Width / _originalWorld.Width;

        public double ScaleBetweenLevels => Scale / Math.Pow(8, DescreetLevel);

        public double Level => Math.Log(Scale, 8);

        public int DescreetLevel => (int) Math.Floor(Level);

        public PointDouble2D FramePosition => new PointDouble2D()
        {
            X = -_currentWorld.X/Scale,
            Y = -_currentWorld.Y/Scale
        };

        public BoundsDouble FrameBounds => new BoundsDouble()
        {
            Width = _originalWorld.Width/Scale,
            Height = _originalWorld.Height/Scale
        };

        public bool DisableScrollingToNegatives { get; set; } = true;

        public event Action<double> LevelScaleChanged = delegate { };

        public void Reset()
        {
            _lastWorld = _originalWorld.Clone();
            _currentWorld = _originalWorld.Clone();
            RaiseLevelScaleChanged();
        }

        private bool _inProgress = false;

        public void Begin()
        {
            if (_inProgress == false)
            {
                _lastWorld = _currentWorld.Clone();

                _inProgress = true;
            }
        }

        public void End()
        {
            _lastWorld = _currentWorld.Clone();

            _inProgress = false;
        }

        public void MoveAndScaleFrame(double scale, PointDouble2D scaleCenter, PointDouble2D offset)
        {
            if (scale < 0)
            {
                scale = 0.1;
            }

            if (DisableScrollingToNegatives && Level <= 0.0001 && scale < 1)
            {
                return;
            }

            var oldLevel = Level;

            var position = scaleCenter.Substract(offset);

            var percentageXOffset = (position.X - _lastWorld.X) / _lastWorld.Width;
            var percentageYOffset = (position.Y - _lastWorld.Y) / _lastWorld.Height;

            var scaledFrame = _lastWorld.Scale(scale);

            var xOffset = (scaledFrame.Width - _lastWorld.Width) * percentageXOffset;
            var yOffset = (scaledFrame.Height - _lastWorld.Height) * percentageYOffset;

            var nx = _lastWorld.X - xOffset + offset.X * scale;
            var ny = _lastWorld.Y - yOffset + offset.Y * scale;

            if (nx < -scaledFrame.Width)
            {
                nx = -scaledFrame.Width;
            }

            if (nx > scaledFrame.Width)
            {
                nx = scaledFrame.Width;
            }

            if (ny < -scaledFrame.Height)
            {
                ny = -scaledFrame.Height;
            }

            if (ny > scaledFrame.Height)
            {
                ny = scaledFrame.Height;
            }

            _currentWorld = new FrameDouble()
            {
                X = nx,
                Y = ny,
                Height = scaledFrame.Height,
                Width = scaledFrame.Width
            };

            if (DisableScrollingToNegatives && Level < 0)
            {
                _currentWorld.Width = _originalWorld.Width;
                _currentWorld.Height = _originalWorld.Height;
            }

            if (Math.Abs(oldLevel - Level) > 0.0001)
            {
                RaiseLevelScaleChanged();
            }
        }

        private void RaiseLevelScaleChanged()
        {
            LevelScaleChanged(Math.Pow(8, Level - (int)Level));
        }

        public ImagePosition[] GetVisibleItems(int level)
        {
            if (level - Level > 0.25f)
            {
                return new ImagePosition[9];
            }

            var part = GetOriginPart(level);

            var topLeft = new ImagePosition()
            {
                Level = level,
                X = (int)Math.Floor(FramePosition.X / part.Width),
                Y = (int)Math.Floor(FramePosition.Y / part.Height)
            };

            var bottomRight = new ImagePosition()
            {
                Level = level,
                X = (int)Math.Floor((FramePosition.X + FrameBounds.Width) / part.Width),
                Y = (int)Math.Floor((FramePosition.Y + FrameBounds.Height) / part.Height)
            };

            var result = new ImagePosition[3, 3];

            int i = 0;
            for (int x = topLeft.X; x <= bottomRight.X; x++)
            {
                var j = 0;
                for (int y = topLeft.Y; y <= bottomRight.Y; y++)
                {
                    result[i, j] = new ImagePosition()
                    {
                        Level = level,
                        X = x,
                        Y = y
                    };
                    j++;
                }
                i++;
            }

            return new ImagePosition[]
            {
                result[0, 0], result[1, 0], result[2, 0],
                result[0, 1], result[1, 1], result[2, 1],
                result[0, 2], result[1, 2], result[2, 2],
            };
        }

        public FrameDouble[] GetWorldPositions(ImagePosition[] positions)
        {
            var result = new FrameDouble[positions.Length];

            for (int i = 0; i < positions.Length; i++)
            {
                var position = positions[i];

                if (position == null)
                {
                    result[i] = null;
                    continue;
                }

                var part = GetWorldPart(position.Level);

                var originalPosition = new FrameDouble()
                {
                    X = _currentWorld.X + part.Width * position.X,
                    Y = _currentWorld.Y + part.Height * position.Y,
                    Height = part.Height,
                    Width = part.Width
                };

                //if (double.IsNaN(originalPosition.Height) || double.IsInfinity(originalPosition.Height))
                //{
                //    originalPosition = null;
                //}

                result[i] = originalPosition;
            }

            return result;
        }

        public PointDouble2D GetWorldPoint(ImagePositionDouble position)
        {
            if (position == null)
            {
                return null;
            }

            var part = GetWorldPart(position.Level);

            var originalPosition = new PointDouble2D()
            {
                X = _currentWorld.X + part.Width * position.X,
                Y = _currentWorld.Y + part.Height * position.Y
            };

            return originalPosition;
        }

        public ImagePositionDouble GetWorldPosition(PointDouble2D screenPoint)
        {
            var x = screenPoint.X - _currentWorld.X;
            var y = screenPoint.Y - _currentWorld.Y;

            var xx = x / _currentWorld.Width;
            var yy = y / _currentWorld.Height;

            return new ImagePositionDouble()
            {
                Level = DescreetLevel,
                X = xx * Math.Pow(8, DescreetLevel),
                Y = yy * Math.Pow(8, DescreetLevel)
            };
        }

        private BoundsDouble GetOriginPart(int level)
        {
            var parts = Math.Pow(8, level);

            var w = _originalWorld.Width / parts;
            var h = _originalWorld.Height / parts;

            return new BoundsDouble()
            {
                Width = w,
                Height = h
            };
        }

        private BoundsDouble GetWorldPart(int level)
        {
            var parts = Math.Pow(8, level);

            var w = _currentWorld.Width / parts;
            var h = _currentWorld.Height / parts;

            return new BoundsDouble()
            {
                Width = w,
                Height = h
            };
        }

        public double GetDistance(PointDouble2D a, PointDouble2D b)
        {
            var x1 = a.X;
            var y1 = a.Y;
            var x2 = b.X;
            var y2 = b.Y;
            var dx = x2 - x1;
            var dy = y2 - y1;

            var d = Math.Sqrt(dx * dx + dy * dy);

            var scaledD = d / Scale;

            return ActualWidthInMeters / 1024 * scaledD;
        }

        public PointDouble2D GetAccuratePosition(PointDouble2D screenPoint)
        {
            return screenPoint.Multiply(Scale).Substract(CurrentWorld.Offset());
        }

        public void Position(BookmarkDto bookmark)
        {
            _currentWorld = bookmark.World.Clone();
            _lastWorld = bookmark.World.Clone();

            RaiseLevelScaleChanged();
        }

        public void Position(FrameDouble bookmark)
        {
            _currentWorld = bookmark.Clone();
            _lastWorld = bookmark.Clone();

            RaiseLevelScaleChanged();
        }
    }
}