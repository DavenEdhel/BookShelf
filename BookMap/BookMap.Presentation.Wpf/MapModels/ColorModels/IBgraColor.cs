namespace BookMap.Presentation.Wpf.InteractionModels
{
    public interface IBgraColor
    {
        uint Bgra { get; }

        public static BgraColorComparer Comparer { get; } = new BgraColorComparer();
    }
}