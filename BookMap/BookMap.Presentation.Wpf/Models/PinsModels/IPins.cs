using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Subjects;
using BookMap.Presentation.Apple.Services;

namespace BookMap.Presentation.Wpf.Models
{
    public interface IPins : IEnumerable<IPin>
    {
        Subject<Unit> Changed { get; }

        void Remove(Guid pinId);

        void Highlight(Guid pinId);

        void ClearHighlighting();

        bool Visible { get; set; }
    }

    public interface IPin
    {
        PinDto Data { get; }

        void Highlight();
    }
}