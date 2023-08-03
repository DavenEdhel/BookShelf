using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Keurig.Tests.Common.Utils
{
    public static class Claim
    {
        public static void ClaimEqual<T>(this T actual, T expected, string message = null) => Equal(actual, expected, message);

        public static void ClaimNotEqual<T>(this T actual, T expected) => NotEqual(actual, expected);

        public static void ClaimFalse(this bool item) => False(item);

        public static void ClaimTrue(this bool item) => True(item);

        public static void Equal(decimal actual, decimal expected)
        {
            Assert.AreEqual(expected, actual);
        }

        public static void Equal<T>(T actual, T expected, string message = null)
        {
            Assert.AreEqual(expected, actual, message);
        }

        public static void Fail(string message)
        {
            Assert.Fail(message);
        }

        public static void True(bool condition)
        {
            Assert.True(condition);
        }

        public static void NotEmpty(string resultVersion)
        {
            Assert.False(string.IsNullOrWhiteSpace(resultVersion));
        }

        public static void IsNotNull(object item)
        {
            Assert.IsNotNull(item);
        }

        public static void CollectionNotEmpty<T>(IEnumerable<T> items)
        {
            Assert.IsNotEmpty(items);
        }

        public static void CollectionEmpty<T>(IEnumerable<T> items)
        {
            Assert.IsEmpty(items);
        }

        public static void False(bool condition)
        {
            Assert.False(condition);
        }

        public static void ShouldContain<T>(IEnumerable<T> items, T item)
        {
            Assert.True(items.Any(x => x.Equals(item)));
        }

        public static void ShouldContain<T>(IEnumerable<T> items, Func<T, bool> predicate)
        {
            Assert.True(items.Any(predicate));
        }

        public static void ShouldNotContain<T>(IEnumerable<T> items, T item)
        {
            Assert.False(items.Any(x => x.Equals(item)));
        }

        public static void ShouldNotContain<T>(IEnumerable<T> items, Func<T, bool> predicate)
        {
            Assert.False(items.Any(predicate));
        }

        public static void NotEqual<T>(T actual, T expected)
        {
            Assert.AreNotEqual(expected, actual);
        }

        public static void IsNull(object item)
        {
            Assert.IsNull(item);
        }

        public static async Task ExpectException(Func<Task> operation)
        {
            var exceptionOccurs = false;

            try
            {
                await operation();
            }
            catch
            {
                exceptionOccurs = true;
            }

            if (exceptionOccurs == false)
            {
                Claim.Fail("Exception is expected.");
            }
        }

        public static void ExpectException(Action operation)
        {
            var exceptionOccurs = false;

            try
            {
                operation();
            }
            catch
            {
                exceptionOccurs = true;
            }

            if (exceptionOccurs == false)
            {
                Claim.Fail("Exception is expected.");
            }
        }

        public static void ExpectExceptionOfType<T>(Action operation)
            where T : Exception
        {
            var exceptionOccurs = false;

            try
            {
                operation();
            }
            catch (T)
            {
                exceptionOccurs = true;
            }
            catch (Exception ex)
            {
                Claim.Fail($"Expected exception of type {typeof(T).Name}, but it was {ex.GetType().Name}.");
            }

            if (exceptionOccurs == false)
            {
                Claim.Fail("Exception is expected.");
            }
        }

        public static async Task<T> ExpectException<T>(Func<Task<T>> operation)
            where T : class
        {
            var exceptionOccurs = false;
            T result = null;

            try
            {
                result = await operation();
            }
            catch
            {
                exceptionOccurs = true;
            }

            if (exceptionOccurs == false)
            {
                Claim.Fail("Exception is expected.");
            }

            return null;
        }
    }
}