namespace BookWiki.Presentation.Wpf.Models.SpellCheckModels
{
    public interface IFileProvider
    {
        string[] ReadAllLines(string filePath);

        void Append(string lexPath, string newWord);
    }
}