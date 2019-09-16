using System;

namespace BookWiki.Core.Files.PathModels
{
    public static class FileSystemPath
    {
        public static IAbsolutePath AbsolutePath(this IRelativePath relative, IRootPath root)
        {
            return new AbsolutePartsPath(root, relative);
        }

        public static IRelativePath RelativePath(this IAbsolutePath absolute, IRootPath root)
        {
            return new RelativePartsPath(absolute, root);
        }
    }
}