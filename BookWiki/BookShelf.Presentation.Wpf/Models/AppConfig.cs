using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json;

namespace BookWiki.Presentation.Wpf.Models
{
    public class AppConfigDto
    {
        public string Root { get; set; }

        public string LibraryPath { get; set; }

        public TabDto[] Tabs { get; set; } = new TabDto[0];

        public string BackupPath { get; set; }

        public int HeightModification { get; set; }
    }

    public class AppConfig
    {
        public AppConfig()
        {
            if (File.Exists("AppConfig.json"))
            {
                Data = JsonConvert.DeserializeObject<AppConfigDto>(File.ReadAllText("AppConfig.json"));
            }
            else
            {
                var config = new AppConfigDto();

                var currentPath = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName;

                config.Root = Path.Combine(currentPath, "Data");
                config.LibraryPath = currentPath;
                config.BackupPath = Path.Combine(currentPath, "Backups");

                if (Directory.Exists(config.Root) == false)
                {
                    Directory.CreateDirectory(config.Root);

                    var novels = Path.Combine(config.Root, "Рассказы");

                    if (Directory.Exists(novels) == false)
                    {
                        Directory.CreateDirectory(novels);
                    }
                }

                if (Directory.Exists(config.LibraryPath) == false)
                {
                    Directory.CreateDirectory(config.LibraryPath);
                }

                if (Directory.Exists(config.BackupPath) == false)
                {
                    Directory.CreateDirectory(config.BackupPath);
                }

                var dictionaryPath = Path.Combine(currentPath, "Russian.lex");
                if (File.Exists(dictionaryPath) == false)
                {
                    File.Create(dictionaryPath);
                }

                Data = config;

                File.WriteAllText(Path.Combine(currentPath, "AppConfig.json"), JsonConvert.SerializeObject(Data, Formatting.Indented));
            }
        }

        public AppConfigDto Data { get; }
    }
}