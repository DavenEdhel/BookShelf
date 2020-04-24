namespace BookWiki.Presentation.Apple.Views
{
    public class BooleanStatesView : SeveralStatesView
    {
        private readonly string _on;
        private readonly string _off;

        public bool IsOff
        {
            get => Current == _off;
            set => Current = value ? _off : _on;
        } 

        public BooleanStatesView(string on, string off) : base(on, off)
        {
            _on = @on;
            _off = off;
        }
    }
}