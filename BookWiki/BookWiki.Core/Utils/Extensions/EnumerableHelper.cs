using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Keurig.IQ.Core.CrossCutting.Extensions
{
    public static class EnumerableHelper
    {
        public static IEnumerable<T> CombineWith<T>(this IEnumerable<T> first, IEnumerable<T> second)
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

        public static bool IsAnyOf<T>(this T item, IEnumerable<T> source)
        {
            return source.Any(x => x.Equals(item));
        }

        public static IEnumerable<T> SelfOrEmpty<T>(this IEnumerable<T> source)
        {
            if (source == null || source.Any() == false)
            {
                return new T[0];
            }

            return source;
        }

        public static bool IsDictionaryEqualsTo(
            this IDictionary<string, string> dictionary1,
            IDictionary<string, string> dictionary2)
        {
            return !(dictionary1.Count != dictionary2.Count || dictionary1.Any(i => dictionary2[i.Key] != i.Value));
        }

        public static string JoinStringsWithoutSkipping(this IEnumerable<string> self, string separator)
        {
            var result = new StringBuilder();

            foreach (var str in self)
            {
                result.Append($"{str}{separator}");
            }

            return result.ToString(0, result.Length - separator.Length);
        }

        public static bool ContainsAny(this IEnumerable<string> self, IEnumerable<string> target)
        {
            return self.ContainsAny(target, (s, t) => s.ToLowerInvariant() == t.ToLowerInvariant());
        }

        public static bool ContainsAny<T>(this IEnumerable<T> self, IEnumerable<T> target, Func<T, T, bool> comparer)
        {
            return target.Any(r => self.Any(ur => comparer(r, ur)));
        }

        public static bool IsNullOrEmptyCollection<T>(this IEnumerable<T> self)
        {
            return self == null || self.Any() == false;
        }

        public static IEnumerable<T> ExceptOf<T>(this IEnumerable<T> self, params T[] ex)
        {
            var exceptCollection = ex.Where(x => x != null).ToArray();

            if (exceptCollection.Any())
            {
                return self.Except(exceptCollection);
            }

            return self;
        }

        public static List<T> ReorderToFront<T>(this List<T> self, Func<T, bool> isItem)
        {
            var updateTile = self.FirstOrDefault(isItem);
            if (updateTile != null)
            {
                self.Remove(updateTile);
                self.Insert(0, updateTile);
            }

            return self;
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> self)
        {
            var items = self.ToArray();

            var rnd = new Random();
            var indexes = new List<int>();

            do
            {
                for (int i = 0; i < items.Length * 10; i++)
                {
                    indexes.Add(rnd.Next(0, items.Length - 1));
                }

                indexes = indexes.Distinct().ToList();
            }
            while (indexes.Count == items.Length);

            var result = new System.Collections.Generic.List<T>();

            foreach (var index in indexes)
            {
                result.Add(items[index]);
            }

            return result.ToArray();
        }
    }
}
