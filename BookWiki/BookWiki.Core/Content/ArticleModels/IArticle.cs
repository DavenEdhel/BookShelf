using System.Collections.Generic;

namespace BookWiki.Core
{
    public interface IArticle : IContent
    {
        void SetViewport(IView articleView);

        void RemovePart(int location, int length);

        void InsertPart(int location, string text);

        IEnumerable<ArticlePart> ToArticleParts();
    }
}