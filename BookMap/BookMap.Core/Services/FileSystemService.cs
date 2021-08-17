using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BookMap.Core.Models;
using Newtonsoft.Json;

namespace BookMap.Presentation.Apple.Services
{
    public class FileSystemService
    {
        public static string DirectoryPath { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        private readonly IImageFactory _imageFactory;
        private readonly List<ImageItem> _modifiedItems = new List<ImageItem>();

        public string MapName { get; private set; }

        public FileSystemService(IImageFactory imageFactory)
        {
            _imageFactory = imageFactory;
            RunAutosave();
        }

        public static void CreateNewMap(string mapName)
        {
            var dirPath = DirectoryPath;

            var newPath = Path.Combine(dirPath, mapName);

            Directory.CreateDirectory(newPath);
        }

        public static IEnumerable<string> GetMapNames()
        {
            var dirPath = DirectoryPath;

            foreach (var dir in Directory.EnumerateDirectories(dirPath))
            {
                yield return dir.Split('/').Last();
            }
        }

        public async Task LoadMap(string mapName)
        {
            await SaveModifiedItems();

            MapName = mapName;
        }

        public MapInfo GetSettings(string mapName)
        {
            var pathToConfig = GetPath(MapName, "config.json");

            if (File.Exists(pathToConfig) == false)
            {
                var result = new MapInfo();

                SaveSettings(mapName, result);

                return result;
            }

            var json = File.ReadAllText(pathToConfig);

            return JsonConvert.DeserializeObject<MapInfo>(json);
        }

        public void SaveSettings(string mapName, MapInfo info)
        {
            var pathToConfig = GetPath(mapName, "config.json");

            var json = JsonConvert.SerializeObject(info);

            File.WriteAllText(pathToConfig, json);
        }

        private async void RunAutosave()
        {
            while (true)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(5));

                    await SaveModifiedItems();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public Task<IImage> TryLoadFromFileSystem(string imageName)
        {
            if (MapName == null || imageName == null)
            {
                return Task.FromResult((IImage)null);
            }

            return LoadFromFileSystemAsync(imageName);
        }

        private Task<IImage> LoadFromFileSystemAsync(string imageName)
        {
            EnsureMapCreated(MapName);

            var filePath = GetPath(MapName, imageName);

            if (File.Exists(filePath) == false)
            {
                return Task.FromResult((IImage) null);
            }

            return Task.Run(() =>
            {
                return _imageFactory.LoadFrom(filePath);
            });
        }

        private void EnsureMapCreated(string mapName)
        {
            var mapPath = GetPath(mapName);

            if (Directory.Exists(mapPath))
            {
                return;
            }

            Directory.CreateDirectory(mapPath);
        }

        private string GetPath(params string[] parts)
        {
            var dirPath = DirectoryPath;

            var partsList = new List<string>();
            partsList.Add(dirPath);
            partsList.AddRange(parts);

            return Path.Combine(partsList.ToArray());
        }

        private ImageItem[] GetItemsToSave()
        {
            lock (this)
            {
                var result = new List<ImageItem>();

                foreach (var modifiedItem in _modifiedItems)
                {
                    result.Add(new ImageItem() {Image = modifiedItem.Image.Copy(), Name = modifiedItem.Name});
                }

                _modifiedItems.Clear();

                return result.ToArray();
            }
        }

        public void Modify(IImage oldImage, IImage newImage, string name)
        {
            lock (this)
            {
                var old = _modifiedItems.FirstOrDefault(x => x.Image.EqualsTo(oldImage));
                if (old != null)
                {
                    _modifiedItems.Remove(old);
                }

                var newItem = new ImageItem() { Image = newImage, Name = name };
                _modifiedItems.Add(newItem);
            }
        }

        private async Task SaveModifiedItems()
        {
            var itemsToSave = GetItemsToSave();

            foreach (var imageItem in itemsToSave)
            {
                await SaveImage(imageItem);
            }
        }

        private Task SaveImage(ImageItem item)
        {
            var path = GetPath(MapName, item.Name);

            return Task.Run(() =>
            {
                var (result, reason) = item.Image.TrySave(path);

                if (result == false)
                {
                    System.Diagnostics.Debug.WriteLine($"Could not save image. Reason {reason}.");
                }
            });
        }

        private void SaveImageSync(ImageItem item)
        {
            var path = GetPath(MapName, item.Name);

            var (result, reason) = item.Image.TrySave(path);

            if (result == false)
            {
                System.Diagnostics.Debug.WriteLine($"Could not save image. Reason {reason}.");
            }
        }

        public void SaveModifiedItemsSync()
        {
            var itemsToSave = GetItemsToSave();

            foreach (var imageItem in itemsToSave)
            {
                SaveImageSync(imageItem);
            }
        }

        public bool Exists(string imageName)
        {
            return File.Exists(GetPath(MapName, imageName));
        }

        public void Remove(string imageName)
        {
            File.Delete(GetPath(MapName, imageName));
        }
    }
}