using System;

namespace Keurig.IQ.Core.CrossCutting.Extensions
{
    public static class ObjectExtensions
    {
        public static T CastTo<T>(this object self)
        {
            try
            {
                return (T)self;
            }
            catch (InvalidCastException e)
            {
                System.Diagnostics.Debug.WriteLine($"Cannot convert {self.GetType().Name} into {typeof(T).Name}.");
                throw;
            }
        }

        public static T As<T>(this object self)
            where T : class
        {
            return self as T;
        }

        public static bool IsNull(this object self)
        {
            return self == null;
        }

        public static bool IsNotNull(this object self)
        {
            return self != null;
        }

        public static bool IsNot<T>(this object self)
        {
            return !(self is T);
        }

        public static T Make<T>(this T item, Action<T> action)
        {
            action(item);

            return item;
        }
    }
}