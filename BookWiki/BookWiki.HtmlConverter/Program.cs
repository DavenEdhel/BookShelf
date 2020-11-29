using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookWiki.Core;
using BookWiki.Core.Files.FileModels;
using BookWiki.Core.Files.PathModels;

namespace BookWiki.HtmlConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            var versionNumber = 43;

            Console.WriteLine("Enter novel path:");
            var novelPath = $@"C:\Users\Stanislau_Kanapliani\iCloudDrive\Book Wiki\Book Wiki {versionNumber}\Рассказы\Важные дела"; //Console.ReadLine();

            var files = Directory.EnumerateDirectories(novelPath, "*.n");

            foreach (var file in files)
            {
                var novel = new ContentFolder(new FolderPath(file));

                var content = novel.LoadText();
                var format = novel.LoadFormat();

                var rtf = new RichFormattedContent(content, format);

                File.WriteAllText(Path.Combine(file, "Publish.html"), rtf.GetHtmlString().PlainText);

                Console.WriteLine($"Publish version for {file} was created.");
            }

            Console.ReadKey();
        }
    }
}
