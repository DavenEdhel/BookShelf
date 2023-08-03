namespace BookWiki.Core.Utils.TextModels
{
    public interface IString
    {
        string Value { get; }
    }

    public class EmptyString : IString
    {
        public string Value { get; } = string.Empty;
    }
}