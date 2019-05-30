using System.Linq;
using BookWiki.Core.Utils.PropertyModels;

namespace BookWiki.Core.Files.PathModels
{
    public class FileName : IFileName
    {
        public FileName(IPartsSequence parts)
        {
            _name = new CachedValue<string>(() => { return parts.Last().SplitBy('.').First().PlainText; });
        }

        public FileName(string fileName)
        {
            _name = new CachedValue<string>(fileName);
        }

        public string PlainText => _name.Value;

        private readonly IProperty<string> _name;
    }
}