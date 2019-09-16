using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BookWiki.Core.Utils.TextModels;

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

        public static IEnumerable<ITextRange> Substract<T>(this ITextRange[] source, ITextRange[] target)
        {
            var s1 = source.ToArray();
            var s2 = target.ToArray();

            var i = 0;

            for (int j = 0; j < s1.Length; j++)
            {
                if (i < s2.Length && s1[j].PlainText ==  s2[i].PlainText)
                {
                    i++;
                    continue;
                }
                else
                {
                    yield return s1[j];
                }
            }
        }
    }
}