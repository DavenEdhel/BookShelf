using System;
using System.Reactive.Subjects;
using BookMap.Presentation.Wpf.Core;

namespace BookMap.Presentation.Wpf.InteractionModels
{
    public class CurrentBrush : IBrush, IObservable<IBrush>
    {
        public int SizeInPixels
        {
            get => _sizeInPixels;
            set
            {
                _sizeInPixels = value;
                ReportChange();
            }
        }

        public IBgraColor Color
        {
            get => _color;
            set
            {
                _color = value;
                ReportChange();
            }
        }

        private ReplaySubject<IBrush> current = new(1);
        private int _sizeInPixels = 50;
        private IBgraColor _color = new BgraColorFromArgb(216, 139, 116);

        public CurrentBrush()
        {
            ReportChange();
        }

        public void Set(IBrush brush)
        {
            SizeInPixels = brush.SizeInPixels;
            Color = brush.Color;

            ReportChange();
        }

        public IDisposable Subscribe(IObserver<IBrush> observer)
        {
            return current.Subscribe(observer);
        }

        private void ReportChange()
        {
            current.OnNext(this.CastTo<IBrush>().Snapshot());
        }
    }

    public interface IBrush
    {
        int SizeInPixels { get; }

        IBgraColor Color { get; }

        public bool EqualsTo(IBrush another)
        {
            return SizeInPixels == another.SizeInPixels && IBgraColor.Comparer.Equals(Color, another.Color);
        }
    }

    public static class BrushApi
    {
        public static IBrush Snapshot(this IBrush brush) => new BrushSnapshot(brush);
    }

    public class BrushSnapshot : IBrush
    {
        public BrushSnapshot(IBrush brush)
        {
            SizeInPixels = brush.SizeInPixels;
            Color = brush.Color;
        }

        public int SizeInPixels { get; }
        public IBgraColor Color { get; }
    }

    public class EraserBrush : IBrush
    {
        public int SizeInPixels { get; } = 10;
        public IBgraColor Color { get; } = new BgraColorFromArgb(0, 0, 0, 0);
    }

    public class DefaultBrush : IBrush
    {
        public int SizeInPixels { get; } = 10;
        public IBgraColor Color { get; } = new BgraColorFromArgb(255, 255, 255, 255);
    }
}