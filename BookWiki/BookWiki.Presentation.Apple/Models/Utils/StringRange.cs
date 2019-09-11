using BookWiki.Core.Utils.TextModels;

namespace BookWiki.Presentation.Apple.Models.Utils
{
    public class StringRange : IRange
    {
        private readonly string _origin;

        public StringRange(string origin)
        {
            _origin = origin;
        }

        public int Length => _origin.Length;

        public int Offset => 0;
    }
}