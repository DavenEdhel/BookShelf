using System.Linq;
using BookWiki.Core.Utils;
using BookWiki.Core.Utils.TextModels;
using Keurig.IQ.Core.CrossCutting.Extensions;

namespace BookWiki.Core.Files.PathModels
{
    public class RelativePartsPath : PartsPath, IRelativePath
    {
        public RelativePartsPath(IAbsolutePath absolutePath, IRootPath rootPath) : base(absolutePath.Parts.ToArray().Substract<ITextRange[]>(rootPath.Parts.ToArray()))
        {
        }
    }
}