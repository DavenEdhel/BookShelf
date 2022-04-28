namespace BookWiki.Core.Utils.TextModels
{
    public class Letter : IString
    {
        private EmpasizingMap _replacementMap = new EmpasizingMap();

        public Letter(string origin)
        {
            IsEmphasized = _replacementMap.ContainsValue(origin);

            Value = origin;
        }

        public bool IsEmphasized { get; }

        public string Value { get; }
    }
}