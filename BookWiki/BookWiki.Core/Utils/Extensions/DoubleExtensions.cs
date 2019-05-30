using System;

namespace Keurig.IQ.Core.CrossCutting.Extensions
{
    public static class DoubleExtensions
    {
        public static bool IsEqualTo(this double a, double b, double lambda)
        {
            return Math.Abs(a - b) < lambda;
        }
    }
}
