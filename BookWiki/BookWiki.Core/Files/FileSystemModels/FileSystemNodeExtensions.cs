using System.Collections.Generic;
using System.Linq;
using BookWiki.Core.Files.PathModels;

namespace BookWiki.Core.Files.FileSystemModels
{
    public static class FileSystemNodeExtensions
    {
        public static List<IFileSystemNode> GetAllLeafs(this IFileSystemNode node)
        {
            var result = new List<IFileSystemNode>();

            AddChildren(node);

            return result;

            void AddChildren(IFileSystemNode current)
            {
                foreach (var currentInnerNode in current.InnerNodes)
                {
                    if (currentInnerNode.IsContentFolder)
                    {
                        result.Add(currentInnerNode);
                    }
                    else
                    {
                        AddChildren(currentInnerNode);
                    }
                }
            }
        }

        public static bool SubitemsArePresentInNavigationPanel(this IFileSystemNode node, IRootPath root, IEnumerable<IContent> openedTabs)
        {
            var openTabPaths = openedTabs.Select(x => x.Source).ToArray();
            var leafPaths = node.GetAllLeafs().Select(x => x.Path.RelativePath(root)).ToArray();

            return leafPaths.Any(x => openTabPaths.Any(y => y.EqualsTo(x)));
        }

        public static bool IsArticlesFolder(this IFileSystemNode node)
        {
            return node.Path.Name.PlainText == "Статьи";
        }

        public static bool IsMapsFolder(this IFileSystemNode node)
        {
            return node.Path.Name.PlainText == "Карты";
        }
    }
}