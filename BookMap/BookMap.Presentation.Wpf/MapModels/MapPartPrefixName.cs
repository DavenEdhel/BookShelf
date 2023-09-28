namespace BookMap.Presentation.Wpf.MapModels
{
    public class MapPartPrefixName
    {
        private readonly bool _isLabel;

        public MapPartPrefixName(bool isLabel)
        {
            _isLabel = isLabel;
        }

        public string PrefixName
        {
            get
            {
                return _isLabel ? "label" : "ground";
            }
        }
    }
}