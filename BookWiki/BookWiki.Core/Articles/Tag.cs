namespace BookWiki.Core.Articles
{
    public class Tag
    {
        public string Name { get; set; }

        public int Usage { get; set; }

        public string[] Relatives { get; set; }
    }
}