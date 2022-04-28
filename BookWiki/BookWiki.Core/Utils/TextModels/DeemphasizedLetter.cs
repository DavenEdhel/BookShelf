namespace BookWiki.Core.Utils.TextModels
{
    public class DeemphasizedLetter : IString
    {
        private EmpasizingMap _replacementMap = new EmpasizingMap();

        public DeemphasizedLetter(string origin)
        {
            if (_replacementMap.ContainsValue(origin))
            {
                Value = _replacementMap.GetKeyByValue(origin);
                IsDeemphasized = true;
            }
            else
            {
                Value = origin;
                IsDeemphasized = false;
            }
        }

        public string Value { get; }

        public bool IsDeemphasized { get; }

    }
}