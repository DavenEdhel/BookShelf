namespace BookWiki.Core.Files.PathModels
{
    public interface IPath
    {
        IFileName Name { get; }

        IExtension Extension { get; }

        string FullPath { get; }

        bool EqualsTo(IPath path);

        IPartsSequence Parts { get; }
    }
}