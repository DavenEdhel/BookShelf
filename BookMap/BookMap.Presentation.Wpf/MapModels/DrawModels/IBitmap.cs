using System;
using System.Windows;
using System.Windows.Media.Imaging;
using BookMap.Presentation.Wpf.InteractionModels;

namespace BookMap.Presentation.Wpf.MapModels.DrawModels
{
    public interface IBitmap
    {
    }

    public static class DrawingApi
    {
        public static void Draw(this WriteableBitmap bitmap, Point position, IBrush brush, IDrawingShape shape = null)
        {
            new BitmapPart(bitmap, position, brush.SizeInPixels).Write(brush.Color, shape ?? new Rectangle());
        }
    }

    public class BitmapPart
    {
        private readonly WriteableBitmap _bitmap;
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
    }

    public readonly struct PositionInRect
    {
        private readonly int _positionInArray;
        private readonly int _size;

        public PositionInRect(int positionInArray, int size)
        {
            _positionInArray = positionInArray;
            _size = size;

            var side = (int)Math.Sqrt(_size);

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
}