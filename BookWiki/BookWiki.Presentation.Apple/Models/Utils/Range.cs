using BookWiki.Core.Utils.TextModels;

namespace BookWiki.Presentation.Apple.Models.Utils
{
    public class Range : IRange
    {
        public Range(int length, int offset)
        {
            Length = length;
            Offset = offset;
        }

        public int Length { get; }
        public int Offset { get; }
    }
}