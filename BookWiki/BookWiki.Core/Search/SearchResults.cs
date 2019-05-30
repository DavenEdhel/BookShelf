using BookWiki.Core.Findings;

namespace BookWiki.Core
{
    public class SearchResult
    {
        public IContent Article { get; }

        public ISequence<IFinding> Findings { get; }

        public SearchResult(IContent article, IFinding[] findings)
        {
            Article = article;
            Findings = new EnumerableSequence<IFinding>(findings, findings.Length);
        }

        public SearchResult(IContent article, IQuery query)
        {
            Article = article;
            Findings = new RunOnceSequence<IFinding>(new FindingsSequence(article, query));
        }

        public SearchResult(IContent article) : this (article, new IFinding[] { new RangeContextFinding(article) })
        {
        }
    }
}