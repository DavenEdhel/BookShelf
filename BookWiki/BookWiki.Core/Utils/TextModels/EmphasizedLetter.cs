using System.Collections.Generic;

namespace BookWiki.Core.Utils.TextModels
{
    public class EmphasizedLetter : IString
    {
        private Dictionary<string, string> _replacementMap = new EmpasizingMap();

        public EmphasizedLetter(string origin)
        {
            if (_replacementMap.ContainsKey(origin))
            {
                Value = _replacementMap[origin];
                IsEmphasized = true;
            }
            else
            {
                Value = origin;
                IsEmphasized = false;
            }
        }

        public string Value { get; }

        public bool IsEmphasized { get; }
    }
}