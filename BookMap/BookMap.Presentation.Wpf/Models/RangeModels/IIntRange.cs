namespace Medbullets.CrossCutting.Data.RangeModels
{
    public interface IIntRange
    {
        bool IncludeLeft { get; }

        bool IncludeRight { get; }

        int Left { get; }

        int Right { get; }
    }
}