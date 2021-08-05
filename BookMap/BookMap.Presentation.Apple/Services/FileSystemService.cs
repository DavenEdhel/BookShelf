using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using Newtonsoft.Json;
using UIKit;

namespace BookMap.Presentation.Apple.Services
{
    public class FileSystemService
    {
        private readonly List<ImageItem> _modifiedItems = new List<ImageItem>();

        public string MapName { get; private set; }

        public FileSystemService()
        {
            RunAutosave();
        }

        public static void CreateNewMap(string mapName)
        {
            var dirPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            var newPath = Path.Combine(dirPath, mapName);

            Directory.CreateDirectory(newPath);
        }

        public static IEnumerable<string> GetMapNames()
        {
            var dirPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

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

        public Task<UIImage> TryLoadFromFileSystem(string imageName)
        {
            if (MapName == null || imageName == null)
            {
                return Task.FromResult((UIImage)null);
            }

            return LoadFromFileSystemAsync(imageName);
        }

        private Task<UIImage> LoadFromFileSystemAsync(string imageName)
        {
            EnsureMapCreated(MapName);

            var filePath = GetPath(MapName, imageName);

            if (File.Exists(filePath) == false)
            {
                return Task.FromResult((UIImage) null);
            }

            return Task.Run(() =>
            {
                try
                {
                    var fileImage = UIImage.FromFile(filePath);

                    var memoryImage = UIImage.FromImage(fileImage.CGImage);

                    fileImage.Dispose();

                    return memoryImage;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
                
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
            var dirPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

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
                    var copiedImage = UIImage.FromImage(modifiedItem.Image.CGImage);

                    result.Add(new ImageItem() {Image = copiedImage, Name = modifiedItem.Name});
                }

                _modifiedItems.Clear();

                return result.ToArray();
            }
        }

        public void Modify(UIImage oldImage, UIImage newImage, string name)
        {
            lock (this)
            {
                var old = _modifiedItems.FirstOrDefault(x => x.Image == oldImage);
                if (old != null)
                {
                    _modifiedItems.Remove(old);
                }

                var newItem = new ImageItem() { Image = newImage, Name = name };
                _modifiedItems.Add(newItem);
            }
        }

        public void Modify(UIImage newImage, string name)
        {
            lock (this)
            {
                var old = _modifiedItems.FirstOrDefault(x => x.Name == name);
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
                var png = item.Image.AsPNG();

                NSError error;

                if (png.Save(path, false, out error) == false)
                {
                    System.Diagnostics.Debug.WriteLine($"Could not save image. Reason {error.LocalizedDescription}.");
                }
            });
        }

        private void SaveImageSync(ImageItem item)
        {
            var path = GetPath(MapName, item.Name);

            var png = item.Image.AsPNG();

            NSError error;

            if (png.Save(path, false, out error) == false)
            {
                System.Diagnostics.Debug.WriteLine($"Could not save image. Reason {error.LocalizedDescription}.");
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