using System;
using System.IO;
using BookWiki.Core.Files.FileSystemModels;

namespace BookWiki.Presentation.Wpf.Models
{
    public class BackupOperation
    {
        private readonly IFileSystemNode _node;

        public BackupOperation(IFileSystemNode node)
        {
            _node = node;
        }

        public void Execute()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(BookShelf.Instance.Config.BackupPath))
                {
                    Result = "Не установлен директорий для бэкапа.";

                    return;
                }

                var dir = GetDirectoryFullPathName();

                CopyFolder(_node.Path.FullPath, dir);

                Result = $"Успешно забэкаплено в {dir}";
            }
            catch (Exception e)
            {
                Result = e.Message;
            }
        }

        public string Result { get; private set; }

        private string GetDirectoryFullPathName()
        {
            var dirNameForBackup = _node.Path.Name.PlainText;

            var expectedDirectory = Path.Combine(BookShelf.Instance.Config.BackupPath, dirNameForBackup);

            var n = 0;

            while (Directory.Exists(expectedDirectory + " " + n.ToString("0000")))
            {
                n++;
            }

            return expectedDirectory + " " + n.ToString("0000");
        }

        public static void CopyFolder(string sourceFolder, string destFolder)
        {
            if (!Directory.Exists(destFolder))
                Directory.CreateDirectory(destFolder);
            string[] files = Directory.GetFiles(sourceFolder);
            foreach (string file in files)
            {
                string name = Path.GetFileName(file);
                if (name.Contains("cover"))
                {
                    continue;
                }
                string dest = Path.Combine(destFolder, name);
                File.Copy(file, dest);
            }
            string[] folders = Directory.GetDirectories(sourceFolder);
            foreach (string folder in folders)
            {
                string name = Path.GetFileName(folder);
                string dest = Path.Combine(destFolder, name);
                CopyFolder(folder, dest);
            }
        }
    }
}