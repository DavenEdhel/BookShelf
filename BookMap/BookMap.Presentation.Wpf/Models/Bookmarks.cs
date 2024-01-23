using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Subjects;
using BookMap.Presentation.Apple.Models;
using BookMap.Presentation.Apple.Services;

namespace BookMap.Presentation.Wpf.Models
{
    public class Bookmarks : IEnumerable<Bookmark>, IObservable<Unit>
    {
        private readonly MapProviderSynchronous _mapProvider;
        private readonly MapView _view;
        private readonly Subject<Unit> _changed = new();

        public Bookmarks(MapProviderSynchronous mapProvider, MapView view)
        {
            _mapProvider = mapProvider;
            _view = view;
        }

        public void Restore(FrameDouble region)
        {
            _view.PositionMapToRegion(region);
        }

        public FrameDouble Current()
        {
            return _view.ExtractBookmark().World;
        }

        public void Make(string bookmarkName)
        {
            var newBookmark = _view.ExtractBookmark();
            newBookmark.Name = bookmarkName;
            _mapProvider.AddBookmark(newBookmark);
            _changed.OnNext(Unit.Default);
        }

        public IEnumerator<Bookmark> GetEnumerator()
        {
            return _mapProvider.Settings.BookmarksV2.Select(x => new Bookmark(x, _mapProvider, _view, _changed)).ToArray().ToList().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IDisposable Subscribe(IObserver<Unit> observer)
        {
            return _changed.Subscribe(observer);
        }
    }

    public class Bookmark
    {
        private readonly BookmarkV2 _dto;
        private readonly MapProviderSynchronous _mapProvider;
        private readonly MapView _view;
        private readonly Subject<Unit> _changed;

        public Bookmark(BookmarkV2 dto, MapProviderSynchronous mapProvider, MapView view, Subject<Unit> changed)
        {
            _dto = dto;
            _mapProvider = mapProvider;
            _view = view;
            _changed = changed;
        }

        public string Name => _dto.Name;

        public void Apply()
        {
            _view.PositionMapToBookmark(_dto);
        }

        public void Remove()
        {
            _mapProvider.RemoveBookmark(_dto.Id);

            _changed.OnNext(Unit.Default);
        }
    }
}