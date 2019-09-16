namespace BookWiki.Core.Files.PathModels
{
    public class AbsolutePartsPath : PartsPath, IAbsolutePath
    {
        public AbsolutePartsPath(IRootPath root, IRelativePath relative) : base(root.Parts, relative.Parts)
        {
        }
    }
}