using BookWiki.Core.Files.PathModels;
using BookWiki.Core.Utils.TextModels;

namespace BookWiki.Core.Fb2Models
{
    public class UniqueName : IString
    {
        public UniqueName(IAbsolutePath pathToFolder, string fileName, string extension)
        {
            var estimated = new FilePath(pathToFolder, $"{fileName}.{extension}");

            if (estimated.HasResource() == false)
            {
                Value = $"{fileName}.{extension}";
            }
            else
            {
                var i = 1;

                do
                {
                    estimated = new FilePath(pathToFolder, $"{fileName} {i:000}.{extension}");
                    Value = $"{fileName} {i:000}.{extension}";
                    i++;

                } while (estimated.HasResource());
            }
        }

        public string Value { get; }
    }
}