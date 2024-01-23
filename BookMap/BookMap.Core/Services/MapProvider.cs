using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookMap.Core.Models;
using BookMap.Presentation.Apple.Models;

namespace BookMap.Presentation.Apple.Services
{
    public class MapProvider
    {
        private const int CacheSize = 60;

        private readonly IImageFactory _imageFactory;

        private readonly List<(string name, IImage image)> _cache = new List<(string name, IImage image)>();

        private readonly Dictionary<string, Task<IImage>> _loadingPool = new Dictionary<string, Task<IImage>>();

        private readonly FileSystemService _fileSystemService;

        public string CurrentMap { get; private set; }

        public event Action<string> MapChanged = delegate { };

        public event Action<MapInfo> SettingsChanged = delegate { };

        public MapProvider(IImageFactory imageFactory)
        {
            _imageFactory = imageFactory;
            _fileSystemService = new FileSystemService(imageFactory);
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

        public async Task LoadMap(string mapName)
        {
            CurrentMap = mapName;

            await _fileSystemService.LoadMap(mapName);

            foreach (var valueTuple in _cache)
            {
                valueTuple.image.Dispose();
            }

            _cache.Clear();
            _loadingPool.Clear();

            MapChanged(mapName);
        }

        public Task<IImage> GetImageAsync(ImagePosition position, bool isLabel)
        {
            if (position == null)
            {
                return Task.FromResult((IImage)null);
            }

            var imageName = position.GetImageName(isLabel);

            var result = TryLoadFromCache(imageName);

            if (result != null)
            {
                return Task.FromResult(result);
            }

            var alreadyLoading = TryGetAwaiter(imageName);

            if (alreadyLoading != null)
            {
                return alreadyLoading;
            }

            var awaiter = GetImage(position, isLabel);

            RegisterAwaiter(imageName, awaiter);

            if (awaiter.IsCompleted == false && awaiter.IsFaulted == false)
            {
                awaiter.ContinueWith(t => UnregisterAwaiter(imageName));
            }
            else
            {
                UnregisterAwaiter(imageName);
            }

            return awaiter;
        }

        private async Task<IImage> GetImage(ImagePosition position, bool isLabel)
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
                    return await LoadImage(imageName, position, isLabel);
                }
                catch (Exception exception)
                {
                    System.Diagnostics.Debug.WriteLine($"RetryCount {i}, Image {imageName}. Exception loading image {exception}.");
                }
            }

            return await MakeEmptyImage();
        }

        private async Task<IImage> LoadImage(string imageName, ImagePosition position, bool isLabel)
        {
            var result = TryLoadFromCache(imageName);

            if (result != null)
            {
                return result;
            }

            result = await TryLoadFromFileSystem(imageName);

            if (result != null)
            {
                RegisterInCache(result, imageName);

                return result;
            }

            if (isLabel)
            {
                result = await MakeEmptyImage();

                RegisterInCache(result, imageName);

                return result;
            }

            if (position.Level == 0)
            {
                result = (position.X == 0 && position.Y == 0) ? _imageFactory.MakeEmpty("#FFFFFF") : await MakeEmptyImage();

                RegisterInCache(result, imageName);

                return result;
            }

            (var level0, var frame) = await GetUpperLevelImage(position, isLabel: false);

            if (level0 == null)
            {
                result = await MakeEmptyImage();
            }
            else
            {
                result = await MakeSubImage(level0, frame);
            }

            RegisterInCache(result, imageName);

            return result;
        }

        private async Task<IImage> TryLoadFromFileSystem(string imageName)
        {
            try
            {
                return await _fileSystemService.TryLoadFromFileSystem(imageName);
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

        private Task<IImage> MakeSubImage(IImage upper, FrameDouble frame)
        {
            return Task.Run(() =>
            {
                lock (this)
                {
                    return upper.MakeSubImage(frame);
                }
            });
        }

        private Task<IImage> MakeEmptyImage()
        {
            return Task.Run(() =>
            {
                return _imageFactory.MakeEmpty();
            });
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

        private async Task<(IImage image, FrameDouble frame)> GetUpperLevelImage(ImagePosition position, bool isLabel)
        {
            if (position.Level > 0)
            {
                var upperImagePosition = position.UpperLevel();

                var image = await GetImage(upperImagePosition, isLabel);

                var normalizedPosition = position.RelativePositionToParent();

                image = image.Copy();

                var bounds = BoundsDouble.ImageSize.DownScale(8);

                var frame = new FrameDouble(bounds).Right(bounds.Width * normalizedPosition.X).Down(bounds.Height * (7 - normalizedPosition.Y));

                return (image, frame);
            }

            return (null, null);
        }

        private Task<IImage> TryGetAwaiter(string imageName)
        {
            if (_loadingPool.ContainsKey(imageName))
            {
                return _loadingPool[imageName];
            }

            return null;
        }

        private void RegisterAwaiter(string imageName, Task<IImage> image)
        {
            _loadingPool[imageName] = image;
        }

        private void UnregisterAwaiter(string imageName)
        {
            _loadingPool.Remove(imageName);
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

        public async Task RedrawAnew(ImagePosition[] positions)
        {
            foreach (var imagePosition in positions)
            {
                var itemFromCache = _cache.FirstOrDefault(x => x.name == imagePosition.GetImageName(isLabel: false));

                (var level0, var frame) = await GetUpperLevelImage(imagePosition, isLabel: false);

                IImage result;

                if (level0 == null)
                {
                    result = await MakeEmptyImage();
                }
                else
                {
                    result = await MakeSubImage(level0, frame);
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

        public void RemoveBookmark(int index)
        {
            ChangeSettings(settings =>
            {
                settings.Bookmarks.RemoveAt(index);
            });
        }

        public void UpdateBookmark(FrameDouble bookmark, int index)
        {
            ChangeSettings(settings => { settings.Bookmarks[index].World = bookmark; });
        }
    }
}