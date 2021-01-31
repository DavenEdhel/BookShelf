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
}