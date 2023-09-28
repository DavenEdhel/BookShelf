using System.Collections.Generic;
using System.Linq;
using BookWiki.Core.Utils;

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

        public static bool CanGoUp(this IRelativePath origin)
        {
            return origin.Parts.Count() > 0;
        }

        public static IRelativePath Up(this IRelativePath origin)
        {
            return new FolderPath(origin.Parts.Take(origin.Parts.Count() - 1).Select(x => x.PlainText).ToArray()) ;
        }

        public static IRelativePath Down(this IRelativePath origin, string fileName)
        {
            return new FolderPath(
                origin.Parts.Select(x => x.PlainText).ToArray().And(
                    new List<string>()
                    {
                        fileName
                    }
                )
            );
        }
    }
}