namespace BookWiki.Core
{
    public class Inline : IInline
    {
        public Inline(IText text, TextStyle textStyle)
        {
            Text = text;
            TextStyle = textStyle;
        }

        public TextStyle TextStyle { get; set; }

        public IText Text { get; set; }
    }

    public class PlainInline : IInline
    {
        public PlainInline(string text)
        {
            Text = new StringText(text);
            TextStyle = TextStyle.None;
        }

        public TextStyle TextStyle { get; set; }

        public IText Text { get; set; }
    }
}