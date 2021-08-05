using System;
using System.Collections.Generic;
using UIKit;

namespace BookMap.Presentation.Apple.Views
{
    public enum ColorPart
    {
        R, G, B, H, S, V
    }

    public static class ColorPartExtensions
    {
        private static IEnumerable<ColorPart> GetRgb()
        {
            yield return ColorPart.R;
            yield return ColorPart.G;
            yield return ColorPart.B;
        }

        private static bool IsRgb(this ColorPart part) => part == ColorPart.R || part == ColorPart.G || part == ColorPart.B;

        private static bool IsHsv(this ColorPart part) => !part.IsRgb();

        public static UIColor ExtractTopColor(this ColorPart part)
        {
            if (part.IsRgb())
            {
                return UIColor.FromRGB(
                    part.ExtractPart(ColorPart.R),
                    part.ExtractPart(ColorPart.G),
                    part.ExtractPart(ColorPart.B));
            }
            else
            {
                return UIColor.FromHSB(
                    part.ExtractPart(ColorPart.H),
                    part.ExtractPart(ColorPart.S),
                    part.ExtractPart(ColorPart.V));
            }
        }

        private static float ExtractPart(this ColorPart current, ColorPart part, float value = 1.0f)
        {
            if (part == current)
            {
                return value;
            }

            return 0.0f;
        }

        public static UIColor SetTo(this UIColor active, ColorPart lockedPart, float value)
        {
            if (lockedPart.IsRgb())
            {
                nfloat r, g, b, a;
                active.GetRGBA(out r, out g, out b, out a);

                nfloat rr, gr, br, ar;

                rr = lockedPart == ColorPart.R ? value : r;
                gr = lockedPart == ColorPart.G ? value : g;
                br = lockedPart == ColorPart.B ? value : b;

                return UIColor.FromRGB(rr, gr, br);
            }
            else
            {
                nfloat h, s, v, a;
                active.GetHSBA(out h, out s, out v, out a);

                nfloat hr, sr, vr, ar;

                hr = lockedPart == ColorPart.H ? value : h;
                sr = lockedPart == ColorPart.S ? value : s;
                vr = lockedPart == ColorPart.V ? value : v;

                return UIColor.FromHSB(hr, sr.FixPart(), vr.FixPart());

            }
        }

        private static nfloat Normalize(this nfloat value)
        {
            if (value < 0) return 0;
            if (value > 1) return 1;
            return value;
        }

        public static UIColor Modify(this UIColor original, ColorPart part, float delta)
        {
            if (part.IsRgb())
            {
                nfloat r, g, b, a;
                original.GetRGBA(out r, out g, out b, out a);

                nfloat rr, gr, br, ar;

                rr = part == ColorPart.R ? r + delta : r;
                gr = part == ColorPart.G ? g + delta : g;
                br = part == ColorPart.B ? b + delta : b;

                return UIColor.FromRGB(rr.Normalize(), gr.Normalize(), br.Normalize()).FixColor();
            }
            else
            {
                nfloat h, s, v, a;
                original.GetHSBA(out h, out s, out v, out a);

                nfloat hr, sr, vr, ar;

                hr = part == ColorPart.H ? h + delta : h;
                sr = part == ColorPart.S ? s + delta : s;
                vr = part == ColorPart.V ? v + delta : v;

                return UIColor.FromHSB(hr.FixPart().Normalize(), sr.FixPart().Normalize(), vr.FixPart().Normalize());

            }
        }

        private static nfloat FixPart(this nfloat value)
        {
            if (value < 0.001)
            {
                return 0.001f;
            }

            return value;
        }

        private static UIColor FixColor(this UIColor original)
        {
            Func<nfloat, bool> isNeedFix = value => Math.Abs(value - 0.001) <= 0;

            nfloat h, s, v, a;
            original.GetHSBA(out h, out s, out v, out a);

            nfloat hr, sr, vr, ar;

            hr = h;
            sr = isNeedFix(s) ? 0.001f : s;
            vr = isNeedFix(v) ? 0.001f : v;

            return UIColor.FromHSB(hr.Normalize(), sr.Normalize(), vr.Normalize());
        }

        public static nfloat ExtractPercentage(this UIColor original, ColorPart part)
        {
            if (part.IsRgb())
            {
                nfloat r, g, b, a;
                original.GetRGBA(out r, out g, out b, out a);

                if (part == ColorPart.R)
                {
                    return r;
                }

                if (part == ColorPart.G)
                {
                    return g;
                }

                if (part == ColorPart.B)
                {
                    return b;
                }
            }
            else
            {
                nfloat h, s, v, a;
                original.GetHSBA(out h, out s, out v, out a);

                if (part == ColorPart.H)
                {
                    return h;
                }

                if (part == ColorPart.S)
                {
                    return s;
                }

                if (part == ColorPart.V)
                {
                    return v;
                }
            }

            return 0;
        }
    }
}