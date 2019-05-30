namespace BookWiki.Core.Findings
{
    public interface IFinding
    {
        ITextRange Result { get; }
        
        ITextRange Context { get; }

        IFinding Normalize();
    }
}