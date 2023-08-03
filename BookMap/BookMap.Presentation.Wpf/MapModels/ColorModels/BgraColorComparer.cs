using System.Collections.Generic;

namespace BookMap.Presentation.Wpf.InteractionModels
{
    public class BgraColorComparer : IEqualityComparer<IBgraColor>
    {
        public bool Equals(IBgraColor x, IBgraColor y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.Bgra == y.Bgra;
        }

        public int GetHashCode(IBgraColor obj)
        {
            return (int) obj.Bgra;
        }
    }
}