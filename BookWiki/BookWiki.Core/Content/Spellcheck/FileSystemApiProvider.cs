using System.IO;

namespace BookWiki.Presentation.Wpf.Models.SpellCheckModels
{
    public class FileSystemApiProvider : IFileProvider
    {
        public string[] ReadAllLines(string filePath)
        {
            return File.ReadAllLines(filePath);
        }

        public void Append(string lexPath, string newWord)
        {
            File.AppendAllLines(lexPath, new[] {newWord});
        }
    }
}