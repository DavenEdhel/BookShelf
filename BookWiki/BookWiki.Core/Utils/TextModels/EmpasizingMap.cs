using System.Collections.Generic;

namespace BookWiki.Core.Utils.TextModels
{
    public class EmpasizingMap : Dictionary<string, string>
    {
        public EmpasizingMap()
        {
            this["а"] = "а́";
            this["о"] = "о́";
            this["у"] = "у́";
            this["и"] = "и́";
            this["э"] = "э́";
            this["ы"] = "ы́";
            this["я"] = "я́";
            this["ё"] = "ё";
            this["ю"] = "ю́";
            this["е"] = "е́";
            this["А"] = "А́";
            this["О"] = "О́";
            this["У"] = "У́";
            this["И"] = "И́";
            this["Э"] = "Э́";
            this["Ы"] = "Ы́";
            this["Я"] = "Я́";
            this["Ё"] = "Ё";
            this["Ю"] = "Ю́";
            this["Е"] = "Е́";
        }

        public bool IsApplicableFor(string value)
        {
            return ContainsKey(value) || ContainsValue(value);
        }

        public string GetKeyByValue(string value)
        {
            foreach (var keyValue in this)
            {
                if (keyValue.Value == value)
                {
                    return keyValue.Key;
                }
            }

            return string.Empty;
        }
    }
}