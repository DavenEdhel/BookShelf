using BookWiki.Core.Files.FileSystemModels;
using BookWiki.Core.Utils.TextModels;

namespace BookWiki.Presentation.Wpf.Models
{
    public class CompilationTitle : IString
    {
        public CompilationTitle(IFileSystemNode node)
        {
            Value = node.Path.Name.PlainText + " << " + "Компиляция";
        }

        public string Value { get; }
    }
}