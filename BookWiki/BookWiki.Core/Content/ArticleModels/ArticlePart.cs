namespace BookWiki.Core
{
    public class ArticlePart
    {
        public MarkerStyle MarkerStyle { get; set; }

        public string Text { get; set; }

        public object Payload { get; set; }

        public static ArticlePart CreateText(string text)
        {
            return new ArticlePart()
            {
                Text = text,
                MarkerStyle = MarkerStyle.Text
            };
        }

        public static ArticlePart CreateHeader(string header)
        {
            return new ArticlePart()
            {
                Text = header,
                MarkerStyle = MarkerStyle.Header
            };
        }

        public static ArticlePart CreateLink(string text, string link)
        {
            return new ArticlePart()
            {
                Text = text,
                Payload = new LinkData(link),
                MarkerStyle = MarkerStyle.Link
            };
        }
    }
}