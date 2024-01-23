using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookMap.Core.Models;
using BookMap.Presentation.Apple.Models;

namespace BookMap.Presentation.Apple.Services
{
    public class MapProviderSynchronous
    {
        private const int CacheSize = 60;

        private readonly IImageFactory _imageFactory;

        private readonly List<(string name, IImage image)> _cache = new List<(string name, IImage image)>();

        private readonly FileSystemServiceSynchronous _fileSystemService;

        public string CurrentMap { get; private set; }

        public event Action<string> MapChanged = delegate { };

        public event Action<MapInfo> SettingsChanged = delegate { };

        public MapProviderSynchronous(IImageFactory imageFactory)
        {
            _imageFactory = imageFactory;
            _fileSystemService = new FileSystemServiceSynchronous(imageFactory);
        }

        public MapInfo Settings
        {
            get
            {
                if (string.IsNullOrWhiteSpace(CurrentMap))
                {
                    return new MapInfo();
                }

                return _fileSystemService.GetSettings(CurrentMap);
            }
            private set
            {
                _fileSystemService.SaveSettings(CurrentMap, value);
                SettingsChanged(value);
            }
        }

        public void ChangeSettings(Action<MapInfo> changeSettings)
        {
            var settings = Settings;
            changeSettings(settings);
            Settings = settings;
        }

        public void LoadMap(string mapName)
        {
            CurrentMap = mapName;

            _fileSystemService.LoadMap(mapName);

            foreach (var valueTuple in _cache)
            {
                valueTuple.image.Dispose();
            }

            _cache.Clear();

            MapChanged(mapName);
        }

        public IImage GetImageAsync(ImagePosition position, bool isLabel)
        {
            if (position == null)
            {
                return _imageFactory.LoadNull();
            }

            var imageName = position.GetImageName(isLabel);

            var result = TryLoadFromCache(imageName);

            if (result != null)
            {
                return result;
            }

            var awaiter = GetImage(position, isLabel);

            return awaiter;
        }

        private IImage GetImage(ImagePosition position, bool isLabel)
        {
            if (position == null)
            {
                return null;
            }

            for (int i = 0; i < 3; i++)
            {
                var imageName = position.GetImageName(isLabel);

                try
                {
                    return LoadImage(imageName, position, isLabel);
                }
                catch (Exception exception)
                {
                    System.Diagnostics.Debug.WriteLine($"RetryCount {i}, Image {imageName}. Exception loading image {exception}.");
                }
            }

            return MakeEmptyImage();
        }

        private IImage LoadImage(string imageName, ImagePosition position, bool isLabel)
        {
            var result = TryLoadFromCache(imageName);

            if (result != null)
            {
                return result;
            }

            result = TryLoadFromFileSystem(imageName);

            if (result != null)
            {
                RegisterInCache(result, imageName);

                return result;
            }

            if (isLabel)
            {
                result = MakeEmptyImage();

                RegisterInCache(result, imageName);

                return result;
            }

            if (position.Level == 0)
            {
                result = (position.X == 0 && position.Y == 0) ? _imageFactory.MakeEmpty("#FFFFFF") : MakeEmptyImage();

                RegisterInCache(result, imageName);

                return result;
            }

            (var level0, var frame) = GetUpperLevelImage(position, isLabel: false);

            if (level0 == null)
            {
                result = MakeEmptyImage();
            }
            else
            {
                result = MakeSubImage(level0, frame);
            }

            RegisterInCache(result, imageName);

            return result;
        }

        private IImage TryLoadFromFileSystem(string imageName)
        {
            try
            {
                return _fileSystemService.TryLoadFromFileSystem(imageName);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private IImage TryLoadFromCache(string imageName)
        {
            if (_cache.Any(x => x.name == imageName))
            {
                return _cache.FirstOrDefault(x => x.name == imageName).image;
            }

            return null;
        }

        private IImage MakeSubImage(IImage upper, FrameDouble frame)
        {
            return upper.MakeSubImage(frame);
        }

        private IImage MakeEmptyImage()
        {
            return _imageFactory.MakeEmpty();
        }

        private void RegisterInCache(IImage result, string imageName)
        {
            var name = imageName;

            if (_cache.Any(x => x.name == name) == false)
            {
                _cache.Add((name, result));

                System.Diagnostics.Debug.WriteLine($"Added {imageName}. Count {_cache.Count}.");
            }

            if (_cache.Count > CacheSize)
            {
                var first = _cache.First();
                _cache.Remove(first);

                System.Diagnostics.Debug.WriteLine($"Disposing {first.name}.");

                first.image.Dispose();
            }
        }

        private (IImage image, FrameDouble frame) GetUpperLevelImage(ImagePosition position, bool isLabel)
        {
            if (position.Level > 0)
            {
                var upperImagePosition = position.UpperLevel();

                var image = GetImage(upperImagePosition, isLabel);

                var normalizedPosition = position.RelativePositionToParent();

                image = image.Copy();

                var bounds = BoundsDouble.ImageSize.DownScale(8);

                var frame = new FrameDouble(bounds).Right(bounds.Width * normalizedPosition.X).Down(bounds.Height * (7 - normalizedPosition.Y));

                return (image, frame);
            }

            return (null, null);
        }

        public void Modify(IImage oldImage, IImage newImage, bool andSaveToFileSystem = true)
        {
            if (_cache.Any(x => x.image.EqualsTo(oldImage)) == false)
            {
                return;
            }

            var old = _cache.First(x => x.image.EqualsTo(oldImage));

            if (andSaveToFileSystem)
            {
                _fileSystemService.Modify(oldImage, newImage, old.name);
            }

            var newItem = (old.name, newImage);
            _cache.Remove(old);
            _cache.Add(newItem);

            oldImage.Dispose();
        }

        public void ClearCache()
        {
            _cache.Clear();
        }

        public void FlushToFileSystem()
        {
            _fileSystemService.SaveModifiedItemsSync();
        }

        public void RedrawAnew(ImagePosition[] positions)
        {
            foreach (var imagePosition in positions)
            {
                var itemFromCache = _cache.FirstOrDefault(x => x.name == imagePosition.GetImageName(isLabel: false));

                (var level0, var frame) = GetUpperLevelImage(imagePosition, isLabel: false);

                IImage result;

                if (level0 == null)
                {
                    result = MakeEmptyImage();
                }
                else
                {
                    result = MakeSubImage(level0, frame);
                }

                if (itemFromCache.name == null)
                {
                    RegisterInCache(result, imagePosition.GetImageName(isLabel: false));
                }
                else
                {
                    Modify(itemFromCache.image, result, andSaveToFileSystem: false);
                }
            }
        }

        public void ApplyDrawUp(ImagePosition[] positions)
        {
            foreach (var imagePosition in positions)
            {
                var fileName = imagePosition.GetImageName(isLabel: false);

                if (_fileSystemService.Exists(fileName))
                {
                    _fileSystemService.Remove(fileName);
                }
            }
        }

        public void CancelDrawUp(ImagePosition[] positions)
        {
            foreach (var imagePosition in positions)
            {
                _cache.RemoveAll(x => x.name == imagePosition.GetImageName(isLabel: false));
            }
        }

        public void AddBookmark(BookmarkDto bookmark)
        {
            ChangeSettings(settings =>
            {
                settings.Bookmarks.Add(bookmark);
            });
        }

        public void AddBookmark(BookmarkV2 bookmark)
        {
            ChangeSettings(settings =>
            {
                settings.BookmarksV2.Add(bookmark);
            });
        }

        public void AddPin(PinDto pin)
        {
            ChangeSettings(settings =>
            {
                settings.Pins.Add(pin);
            });
        }

        public void RemoveBookmark(int index)
        {
            ChangeSettings(settings =>
            {
                settings.Bookmarks.RemoveAt(index);
            });
        }

        public void RemoveBookmark(Guid id)
        {
            ChangeSettings(settings =>
            {
                settings.BookmarksV2.RemoveAll(x => x.Id == id);
            });
        }

        public void UpdateBookmark(FrameDouble bookmark, int index)
        {
            ChangeSettings(settings => { settings.Bookmarks[index].World = bookmark; });
        }

        public void UpdateBookmark(Guid id, FrameDouble bookmark)
        {
            ChangeSettings(settings => { settings.BookmarksV2.First(x => x.Id == id).World = bookmark; });
        }

        public void RemovePin(Guid pinId)
        {
            ChangeSettings(settings =>
            {
                settings.Pins.RemoveAll(x => x.Id == pinId);
            });
        }
    }
}