using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using BookMap.Presentation.Apple.Extentions;
using BookMap.Presentation.Apple.Models;
using CoreGraphics;
using UIKit;

namespace BookMap.Presentation.Apple.Services
{
    public class MapProvider
    {
        private const int CacheSize = 60;

        private readonly List<(string name, UIImage image)> _cache = new List<(string name, UIImage image)>();

        private readonly Dictionary<string, Task<UIImage>> _loadingPool = new Dictionary<string, Task<UIImage>>();

        private readonly FileSystemService _fileSystemService = new FileSystemService();

        public string CurrentMap { get; private set; }

        public event Action<string> MapChanged = delegate { };

        public event Action<MapInfo> SettingsChanged = delegate { };

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

        public Task<UIImage> GetImageAsync(ImagePosition position, bool isLabel)
        {
            if (position == null)
            {
                return Task.FromResult((UIImage)null);
            }

            var imageName = ImageHelper.GetImageName(position, isLabel);

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

        private async Task<UIImage> GetImage(ImagePosition position, bool isLabel)
        {
            if (position == null)
            {
                return null;
            }

            for (int i = 0; i < 3; i++)
            {
                var imageName = ImageHelper.GetImageName(position, isLabel);

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

        private async Task<UIImage> LoadImage(string imageName, ImagePosition position, bool isLabel)
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
                result =  (position.X == 0 && position.Y == 0) ? UIColor.White.MakeEmptyImage() : await MakeEmptyImage();

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

        private async Task<UIImage> TryLoadFromFileSystem(string imageName)
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

        private UIImage TryLoadFromCache(string imageName)
        {
            if (_cache.Any(x => x.name == imageName))
            {
                return _cache.FirstOrDefault(x => x.name == imageName).image;
            }

            return null;
        }

        private Task<UIImage> MakeSubImage(UIImage upper, FrameDouble frame)
        {
            return Task.Run(() =>
            {
                lock (this)
                {
                    UIGraphics.BeginImageContext(frame.ToSize());
                    //UIGraphics.BeginImageContext(upper.Size);

                    var context = UIGraphics.GetCurrentContext();

                    context.TranslateCTM(0f, (nfloat)frame.Height);
                    context.ScaleCTM(1.0f, -1.0f);

                    context.DrawImage(new CGRect(-(float)frame.X, -(float)frame.Y, upper.CGImage.Width, upper.CGImage.Height), upper.CGImage);
                    //context.DrawImage(new CGRect(0, 0, upper.CGImage.Width, upper.CGImage.Height), upper.CGImage);

                    context.ScaleCTM(1.0f, -1.0f);
                    context.TranslateCTM(0f, -(nfloat)frame.Height);

                    var raw = UIGraphics.GetImageFromCurrentImageContext();

                    var result = raw.Scale(upper.Size);

                    raw.Dispose();

                    context.Dispose();

                    UIGraphics.EndImageContext();

                    return result;
                }
            });
        }

        private Task<UIImage> MakeEmptyImage()
        {
            return Task.Run(() =>
            {
                return ImageHelper.MakeEmptyImage();
            });
        }

        private void RegisterInCache(UIImage result, string imageName)
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

        private async Task<(UIImage image, FrameDouble frame)> GetUpperLevelImage(ImagePosition position, bool isLabel)
        {
            if (position.Level > 0)
            {
                var upperImagePosition = position.UpperLevel();

                var image = await GetImage(upperImagePosition, isLabel);

                var normalizedPosition = position.RelativePositionToParent();

                image = UIImage.FromImage(image.CGImage);

                var bounds = image.Size.ToBounds().DownScale(8);

                var frame = new FrameDouble(bounds).Right(bounds.Width * normalizedPosition.X).Down(bounds.Height * (7 - normalizedPosition.Y));

                return (image, frame);
            }

            return (null, null);
        }

        private Task<UIImage> TryGetAwaiter(string imageName)
        {
            if (_loadingPool.ContainsKey(imageName))
            {
                return _loadingPool[imageName];
            }

            return null;
        }

        private void RegisterAwaiter(string imageName, Task<UIImage> image)
        {
            _loadingPool[imageName] = image;
        }

        private void UnregisterAwaiter(string imageName)
        {
            _loadingPool.Remove(imageName);
        }

        public void Modify(UIImage oldImage, UIImage newImage, bool andSaveToFileSystem = true)
        {
            if (_cache.Any(x => x.image == oldImage) == false)
            {
                return;
            }

            var old = _cache.First(x => x.image == oldImage);

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
                var itemFromCache = _cache.FirstOrDefault(x => x.name == ImageHelper.GetImageName(imagePosition, isLabel: false));

                (var level0, var frame) = await GetUpperLevelImage(imagePosition, isLabel: false);

                UIImage result;

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
                    RegisterInCache(result, ImageHelper.GetImageName(imagePosition, isLabel: false));
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
                var fileName = ImageHelper.GetImageName(imagePosition, isLabel: false);

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
                _cache.RemoveAll(x => x.name == ImageHelper.GetImageName(imagePosition, isLabel: false));
            }
        }

        public void AddBookmark(Bookmark bookmark)
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