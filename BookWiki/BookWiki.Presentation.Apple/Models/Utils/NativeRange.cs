using BookWiki.Core.Utils.TextModels;
using Foundation;

namespace BookWiki.Presentation.Apple.Models.Utils
{
    public class NativeRange : IRange
    {
        private readonly NSRange _range;

        public NativeRange(NSRange range)
        {
            _range = range;
        }

        public int Length => (int)_range.Length;

        public int Offset => (int) _range.Location;
    }
}