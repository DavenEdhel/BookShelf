using System;
using System.Windows;
using System.Windows.Media.Imaging;
using BookMap.Presentation.Wpf.InteractionModels;
using Medbullets.CrossCutting.Data.RangeModels;

namespace BookMap.Presentation.Wpf.MapModels.DrawModels
{
    public interface IBitmap
    {
    }

    public static class DrawingApi
    {
        public static DrawingResult Draw(this WriteableBitmap bitmap, Point position, IBrush brush, IDrawingShape shape = null)
        {
            var s = shape ?? new Circle();

            var operation = new WriteShapeOnBitmapV3(
                bitmap,
                position,
                new Size(brush.SizeInPixels, brush.SizeInPixels),
                x =>
                {
                    var shouldColorize = s.Hit(brush.SizeInPixels, x.X, x.Y);
                    if (shouldColorize)
                    {
                        return (true, brush.Color.Bgra);
                    }

                    return (false, 0);
                }
            );

            operation.Execute();

            return operation.Result;
        }

        public static DrawingResult Draw(this WriteableBitmap bitmap, Point position, Size size, uint[] texture)
        {
            var operation = new WriteShapeOnBitmapV3(
                bitmap,
                position,
                size,
                x => (true, texture[x.Y * (int)size.Width + x.X])
            );

            operation.Execute();

            return operation.Result;
        }

        public static void Draw(this WriteableBitmap bitmap, Point position, RenderTargetBitmap bmp)
        {
            new BitmapPart(bitmap, position, bmp.PixelWidth, bmp.PixelHeight).Write(bmp);
        }
    }

    public class DrawingResult
    {
        public bool CroppedRight { get; set; }

        public bool CroppedTop { get; set; }

        public bool CroppedBottom { get; set; }

        public bool CroppedLeft { get; set; }
    }

    public class WriteShapeOnBitmapV3
    {
        private readonly WriteableBitmap _bitmap;
        private readonly Point _position;
        private readonly Size _region;
        private readonly Func<(int X, int Y), (bool, uint)> _getColor;

        public WriteShapeOnBitmapV3(WriteableBitmap bitmap, Point position, Size region, Func<(int X, int Y), (bool, uint)> getColor)
        {
            _bitmap = bitmap;
            _position = position;
            _region = region;
            _getColor = getColor;
        }

        public DrawingResult Result { get; } = new();

        public void Execute()
        {
            var positionX = 0;
            var deltaX = 0;
            if ((int)_position.X >= 0)
            {
                positionX = (int)_position.X;
                deltaX = 0;
            }
            else
            {
                positionX = 0;
                deltaX = (int)Math.Abs(_position.X);
            }

            var positionY = 0;
            var deltaY = 0;
            if ((int)_position.Y >= 0)
            {
                positionY = (int)_position.Y;
                deltaY = 0;
            }
            else
            {
                positionY = 0;
                deltaY = (int)Math.Abs(_position.Y);
            }

            var width = 0;
            if (_position.X < 0)
            {
                width = (int)_region.Width - (int)Math.Abs(_position.X);

                Result.CroppedLeft = true;
            }
            else if ((int)_position.X + (int)_region.Width > _bitmap.PixelWidth)
            {
                width = (int)_region.Width - ((int)_position.X + (int)_region.Width - _bitmap.PixelWidth);

                Result.CroppedRight = true;
            }
            else
            {
                width = (int)_region.Width;
            }

            var height = 0;
            if (_position.Y < 0)
            {
                height = (int)_region.Height - (int)Math.Abs(_position.Y);

                Result.CroppedTop = true;
            }
            else if ((int)_position.Y + (int)_region.Height > _bitmap.PixelHeight)
            {
                height = (int)_region.Height - ((int)_position.Y + (int)_region.Height - _bitmap.PixelHeight);

                Result.CroppedBottom = true;
            }
            else
            {
                height = (int)_region.Height;
            }

            var area = new Int32Rect(positionX, positionY, width, height);
            var part = new uint[width * height];
            var stride = width * 4;
            var offset = 0;

            _bitmap.CopyPixels(area, part, stride, offset);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var (needToColorize, color) = _getColor((x + deltaX, y + deltaY));

                    if (needToColorize)
                    {
                        part[y * width + x] = color;
                    }
                }
            }

            _bitmap.WritePixels(area, part, stride, offset);
        }
    }

    public class WriteShapeOnBitmapV2
    {
        private readonly WriteableBitmap _bitmap;
        private readonly Point _position;
        private readonly IBrush _brush;
        private readonly IDrawingShape _shape;

        public WriteShapeOnBitmapV2(WriteableBitmap bitmap, Point position, IBrush brush, IDrawingShape shape = null)
        {
            _bitmap = bitmap;
            _position = position;
            _brush = brush;
            _shape = shape ?? new Circle();
        }

        public DrawingResult Result { get; } = new();

        public void Execute()
        {
            var positionX = 0;
            var deltaX = 0;
            if ((int) _position.X >= 0)
            {
                positionX = (int) _position.X;
                deltaX = 0;
            }
            else
            {
                positionX = 0;
                deltaX = (int)Math.Abs(_position.X);
            }

            var positionY = 0;
            var deltaY = 0;
            if ((int)_position.Y >= 0)
            {
                positionY = (int)_position.Y;
                deltaY = 0;
            }
            else
            {
                positionY = 0;
                deltaY = (int)Math.Abs(_position.Y);
            }

            var width = 0;
            if (_position.X < 0)
            {
                width = _brush.SizeInPixels - (int) Math.Abs(_position.X);

                Result.CroppedLeft = true;
            }
            else if ((int) _position.X + _brush.SizeInPixels > _bitmap.PixelWidth)
            {
                width = _brush.SizeInPixels - ((int) _position.X + _brush.SizeInPixels - _bitmap.PixelWidth);

                Result.CroppedRight = true;
            }
            else
            {
                width = _brush.SizeInPixels;
            }

            var height = 0;
            if (_position.Y < 0)
            {
                height = _brush.SizeInPixels - (int)Math.Abs(_position.Y);

                Result.CroppedTop = true;
            }
            else if ((int)_position.Y + _brush.SizeInPixels > _bitmap.PixelHeight)
            {
                height = _brush.SizeInPixels - ((int)_position.Y + _brush.SizeInPixels - _bitmap.PixelHeight);

                Result.CroppedBottom = true;
            }
            else
            {
                height = _brush.SizeInPixels;
            }
            
            var area = new Int32Rect(positionX, positionY, width, height);
            var part = new uint[width * height];
            var stride = width * 4;
            var offset = 0;

            _bitmap.CopyPixels(area, part, stride, offset);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (_shape.Hit(_brush.SizeInPixels, x + deltaX, y + deltaY))
                    {
                        part[y * width + x] = _brush.Color.Bgra;
                    }
                }
            }

            //for (int i = 0; i < _part.Length; i++)
            //{
            //    var position = new PositionInRect2(i, _brush.SizeInPixels);

            //    if (_shape.Hit(_size, position.X, position.Y))
            //    {
            //        _part[i] = _brush.Color.Bgra;
            //    }
            //}

            _bitmap.WritePixels(area, part, stride, offset);
        }
    }

    public class BitmapPart
    {
        private readonly WriteableBitmap _bitmap;
        private readonly int _width;
        private readonly int _height;
        private readonly int _size;
        private readonly Int32Rect _area;
        private readonly uint[] _part;
        private readonly int _stride;
        private readonly int _offset;

        public BitmapPart(WriteableBitmap bitmap, Point position, int size)
        {
            _bitmap = bitmap;
            _size = size;
            _area = new Int32Rect((int)position.X, (int)position.Y, size, size);
            _part = new uint[size * size];
            _stride = size * 4;
            _offset = 0;

            bitmap.CopyPixels(_area, _part, _stride, _offset);
        }

        public BitmapPart(WriteableBitmap bitmap, Point position, int width, int height)
        {
            _bitmap = bitmap;
            _width = width;
            _height = height;
            _size = width * height;
            _area = new Int32Rect((int)position.X, (int)position.Y, width, height);
            _part = new uint[width * height];
            _stride = width * 4;
            _offset = 0;

            bitmap.CopyPixels(_area, _part, _stride, _offset);
        }

        public void Write(IBgraColor color, IDrawingShape shape)
        {
            for (int i = 0; i < _part.Length; i++)
            {
                var position = new PositionInRect(i, _part.Length);

                if (shape.Hit(_size, position.X, position.Y))
                {
                    _part[i] = color.Bgra;
                }
            }

            _bitmap.WritePixels(_area, _part, _stride, _offset);
        }

        public void Write(RenderTargetBitmap bmp)
        {
            var area = new Int32Rect(0, 0, bmp.PixelWidth, bmp.PixelHeight);
            var part = new uint[bmp.PixelWidth * bmp.PixelHeight];
            var stride = bmp.PixelWidth * 4;

            bmp.CopyPixels(area, part, stride, 0);

            _bitmap.WritePixels(_area, part, _stride, _offset);
        }
    }

    public readonly struct PositionInRect
    {
        private readonly int _positionInArray;
        private readonly int _bufferSize;

        public PositionInRect(int positionInArray, int bufferSize)
        {
            _positionInArray = positionInArray;
            _bufferSize = bufferSize;

            var side = (int)Math.Sqrt(_bufferSize);

            Y = positionInArray / side;
            X = positionInArray - (Y * side);
        }

        public int X { get; }

        public int Y { get; }
    }

    public readonly struct PositionInRect2
    {
        public PositionInRect2(int positionInArray, int side)
        {
            Y = positionInArray / side;
            X = positionInArray - (Y * side);
        }

        public int X { get; }

        public int Y { get; }
    }

    public interface IDrawingShape
    {
        bool Hit(int sideLength, int x, int y);
    }

    public interface IRect<T>
    {
        void Value(int x, int y, T value);

        T Value(int x, int y);
    }

    public class PlainArrayAsRect<T> : IRect<T>
    {
        private readonly T[] _array;
        private readonly int _size;

        public PlainArrayAsRect(T[] array)
        {
            _array = array;
            _size = (int) Math.Sqrt(array.Length);
        }

        public void Value(int x, int y, T value)
        {
            _array[y * _size + x] = value;
        }

        public T Value(int x, int y)
        {
            return _array[y * _size + x];
        }
    }

    public class Rectangle : IDrawingShape
    {
        public bool Hit(int sideLength, int x, int y)
        {
            return true;
        }
    }

    public class Circle : IDrawingShape
    {
        public bool Hit(int sideLength, int x, int y)
        {
            var r = sideLength / 2;
            var rx = r;
            var ry = r;

            var s = Math.Sqrt(Math.Pow(x - rx, 2) + Math.Pow(y - ry, 2));

            if (s <= r)
            {
                return true;
            }

            return false;
        }
    }

    public class IBeam : IDrawingShape
    {
        public bool Hit(int sideLength, int x, int y)
        {
            var beamWidth = 0.1f;

            var beamWidthPx = sideLength * beamWidth;
            var half = beamWidthPx / 2;

            var center = sideLength / 2;

            if (x > center - half && x < center + half)
            {
                return true;
            }

            return false;
        }
    }
}