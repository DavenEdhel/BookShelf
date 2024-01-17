using System.Linq;

namespace Medbullets.CrossCutting.Data.RangeModels
{
    public class MathIntRange : IIntRange
    {
        public MathIntRange(int left, int right, bool includeLeft, bool includeRight)
        {
            IncludeLeft = includeLeft;
            IncludeRight = includeRight;
            Left = left;
            Right = right;
        }

        public bool IncludeLeft { get; }

        public bool IncludeRight { get; }

        public int Left { get; }

        public int Right { get; }

        public static MathIntRange Parse(string value)
        {
            value = value.Trim();
            var f = value.First();
            var l = value.Last();
            var inner = value.Substring(1, value.Length - 2).Trim();
            var parts = inner.Split(',').Select(x =>
                {
                    var trimmed = x.Trim();

                    if (trimmed == "-inf")
                    {
                        return int.MinValue;
                    }

                    if (trimmed == "inf")
                    {
                        return int.MaxValue;
                    }

                    return int.Parse(trimmed);
                }
            ).ToArray();

            return new MathIntRange(parts[0], parts[1], f == '[', l == ']');
        }
    }
}