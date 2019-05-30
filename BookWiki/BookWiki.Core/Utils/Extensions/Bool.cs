using System;

namespace Keurig.IQ.Core.CrossCutting.Extensions
{
    public static class Bool
    {
        public const bool On = true;

        public const bool Off = false;

        public const bool Yes = true;

        public const bool No = false;

        public static bool OnlyOneIsTrue(bool first, bool second) => first ^ second;
    }

    public static class InitializationExtensions
    {
        public static Func<TOut> SelfOr<TOut>(this Func<TOut> self, Func<TOut> d)
        {
            return self ?? d;
        }

        public static Func<T1, TOut> SelfOr<T1, TOut>(this Func<T1, TOut> self, Func<T1, TOut> d)
        {
            return self ?? d;
        }
    }
}