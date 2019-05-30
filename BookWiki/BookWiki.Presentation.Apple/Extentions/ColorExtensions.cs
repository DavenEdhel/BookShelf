using System;
using UIKit;

namespace BookMap.Presentation.Apple.Extentions
{
    public static class ColorExtensions
    {
        public static UIColor ToUIColor(this string hexColor)
        {
            if (string.IsNullOrWhiteSpace(hexColor))
                throw new ArgumentException("Invalid color string.", nameof(hexColor));
            if (!hexColor.StartsWith("#", StringComparison.Ordinal))
                hexColor = hexColor.Insert(0, "#");
            UIColor clear = UIColor.Clear;
            switch (hexColor.Length)
            {
                case 4:
                    ushort uint16_1 = Convert.ToUInt16(hexColor.Substring(1), 16);
                    byte num1 = (byte)((int)uint16_1 >> 8 & 15);
                    byte num2 = (byte)((int)uint16_1 >> 4 & 15);
                    byte num3 = (byte)((uint)uint16_1 & 15U);
                    return UIColor.FromRGBA((byte)((uint)num1 << 4 | (uint)num1), (byte)((uint)num2 << 4 | (uint)num2), (byte)((uint)num3 << 4 | (uint)num3), byte.MaxValue);
                case 5:
                    ushort uint16_2 = Convert.ToUInt16(hexColor.Substring(1), 16);
                    byte num4 = (byte)((uint)uint16_2 >> 12);
                    byte num5 = (byte)((int)uint16_2 >> 8 & 15);
                    byte num6 = (byte)((int)uint16_2 >> 4 & 15);
                    byte num7 = (byte)((uint)uint16_2 & 15U);
                    byte alpha1 = (byte)((uint)num4 << 4 | (uint)num4);
                    return UIColor.FromRGBA((byte)((uint)num5 << 4 | (uint)num5), (byte)((uint)num6 << 4 | (uint)num6), (byte)((uint)num7 << 4 | (uint)num7), alpha1);
                case 7:
                    uint uint32_1 = Convert.ToUInt32(hexColor.Substring(1), 16);
                    return UIColor.FromRGBA((byte)(uint32_1 >> 16 & (uint)byte.MaxValue), (byte)(uint32_1 >> 8 & (uint)byte.MaxValue), (byte)(uint32_1 & (uint)byte.MaxValue), byte.MaxValue);
                case 9:
                    uint uint32_2 = Convert.ToUInt32(hexColor.Substring(1), 16);
                    byte alpha2 = (byte)(uint32_2 >> 24);
                    return UIColor.FromRGBA((byte)(uint32_2 >> 16 & (uint)byte.MaxValue), (byte)(uint32_2 >> 8 & (uint)byte.MaxValue), (byte)(uint32_2 & (uint)byte.MaxValue), alpha2);
                default:
                    throw new FormatException(string.Format("The {0} string passed in the c argument is not a recognized Color format.", (object)hexColor));
            }
        }

        public static string ToHexString(this UIColor color)
        {
            nfloat r, g, b, a;

            color.GetRGBA(out r, out g, out b, out a);

            byte rb, gb, bb;

            rb = (byte) (r * 255);
            gb = (byte)(g * 255);
            bb = (byte)(b * 255);

            return $"#{rb:X2}{gb:X2}{bb:X2}";
        }
    }
}