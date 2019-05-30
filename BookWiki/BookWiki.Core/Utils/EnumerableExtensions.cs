using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BookWiki.Core.Utils
{
    public static class EnumerableExtensions
    {
        public static T SecondOrFirst<T>(this IEnumerable<T> self) => self.Count() > 1 ? self.ElementAt(1) : self.First();

        public static int IndexOf<T>(this IEnumerable<T> self, Func<T, bool> condition)
        {
            var i = 0;

            foreach (var item in self)
            {
                if (condition(item))
                {
                    return i;
                }

                i++;
            }

            return -1;
        }

        public static IEnumerable<T> And<T>(this IEnumerable<T> first, IEnumerable<T> second)
        {
            foreach (var f in first)
            {
                yield return f;
            }

            foreach (var s in second)
            {
                yield return s;
            }
        }

        public static int MaxOrDefault<T>(this IEnumerable<T> self, Func<T, int> condition)
        {
            if (self.Any())
            {
                return self.Max(condition);
            }

            return 0;
        }
    }
}