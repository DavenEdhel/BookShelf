using BookWiki.Core.Utils.TextModels;

namespace BookWiki.Core
{
    public class MutableRange : IRange
    {
        public int Length { get; set; }
        public int Offset { get; set; }
    }
}