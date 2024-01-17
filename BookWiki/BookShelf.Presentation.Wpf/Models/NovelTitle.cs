using System.Linq;
using BookWiki.Core;
using BookWiki.Core.Files.PathModels;

namespace BookWiki.Presentation.Wpf.Models
{
    public class NovelTitle
    {
        public NovelTitle(IPath path)
        {
            PlainText = path.Name.PlainText + (path.Parts.Count() > 1 ? (" << " + path.Parts.Reverse().Skip(1).First().PlainText) : "");
        }

        public string PlainText { get; private set; }
    }

    public class ArticleTitle
    {
        public ArticleTitle(Article article)
        {
            PlainText = article.Name + " << Статья";
        }

        public string PlainText { get; private set; }
    }
}