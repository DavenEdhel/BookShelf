namespace BookWiki.Core.Utils.TextModels
{
    public class SimpleRange : IRange
    {
        public SimpleRange(int length, int offset)
        {
            Length = length;
            Offset = offset;
        }

        public static SimpleRange CreateFromStartAndEnd(int start, int end)
        {
            return new SimpleRange(end - start + 1, start);
        }

        public int Length { get; }
        public int Offset { get; }
    }
}