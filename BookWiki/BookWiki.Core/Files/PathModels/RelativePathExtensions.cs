using System.Linq;

namespace BookWiki.Core.Files.PathModels
{
    public static class RelativePathExtensions
    {
        public static bool Contains(this IRelativePath origin, IRelativePath subpath)
        {
            if (subpath.Parts.Count() <= origin.Parts.Count())
            {
                for (int i = 0; i < subpath.Parts.Count(); i++)
                {
                    if (origin.Parts.ElementAt(i).PlainText != subpath.Parts.ElementAt(i).PlainText)
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }
    }
}