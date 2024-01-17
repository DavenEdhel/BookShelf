namespace Medbullets.CrossCutting.Data.RangeModels
{
    public static class RangeExtensions
    {
        public static bool In(this int i, string range)
        {
            var r = MathIntRange.Parse(range);

            return i.In(r);
        }

        public static bool In(this int i, IIntRange r)
        {
            var leftSatisfied = (r.IncludeLeft ? i >= r.Left : i > r.Left);
            var rightSatisfied = (r.IncludeRight ? i <= r.Right : i < r.Right);

            return leftSatisfied && rightSatisfied;
        }

        public static bool In(this float i, IIntRange r)
        {
            var leftSatisfied = (r.IncludeLeft ? i >= r.Left : i > r.Left);
            var rightSatisfied = (r.IncludeRight ? i <= r.Right : i < r.Right);

            return leftSatisfied && rightSatisfied;
        }

        public static float MakeItIn(this float i, string range)
        {
            var r = MathIntRange.Parse(range);

            return i.MakeItIn(r);
        }

        public static float MakeItIn(this float f, IIntRange r)
        {
            if (r.IncludeLeft)
            {
                if (f < r.Left)
                {
                    return r.Left;
                }
            }

            if (r.IncludeLeft == false)
            {
                if (f <= r.Left)
                {
                    return r.Left + 0.0001f;
                }
            }

            if (r.IncludeRight)
            {
                if (f > r.Right)
                {
                    return r.Right;
                }
            }

            if (r.IncludeRight == false)
            {
                if (f >= r.Right)
                {
                    return r.Right - 0.0001f;
                }
            }

            return f;
        }

        public static int MakeItIn(this int f, string r)
        {
            var range = MathIntRange.Parse(r);

            return f.MakeItIn(range);
        }

        public static int MakeItIn(this int f, IIntRange r)
        {
            if (r.IncludeLeft)
            {
                if (f < r.Left)
                {
                    return r.Left;
                }
            }

            if (r.IncludeLeft == false)
            {
                if (f <= r.Left)
                {
                    return r.Left + 1;
                }
            }

            if (r.IncludeRight)
            {
                if (f > r.Right)
                {
                    return r.Right;
                }
            }

            if (r.IncludeRight == false)
            {
                if (f >= r.Right)
                {
                    return r.Right - 1;
                }
            }

            return f;
        }
    }
}