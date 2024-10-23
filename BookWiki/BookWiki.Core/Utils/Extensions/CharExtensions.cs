using System.Linq;

namespace Keurig.IQ.Core.CrossCutting.Extensions
{
    public static class CharExtensions
    {
        public static bool IsHex(this char c)
        {
            return (c >= '0' && c <= '9') ||
                   (c >= 'a' && c <= 'f') ||
                   (c >= 'A' && c <= 'F');
        }

        public static bool IsIn(this char c, params char[] m)
        {
            return m.Contains(c);
        }
    }
}