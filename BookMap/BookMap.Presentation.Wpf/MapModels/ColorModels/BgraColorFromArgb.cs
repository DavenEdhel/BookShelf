using System;
using System.Drawing;
using System.Net.Http.Headers;

namespace BookMap.Presentation.Wpf.InteractionModels
{
    public class BgraColorFromHex : IBgraColor
    {
        public BgraColorFromHex(string hex)
        {
            var c = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(hex);
            Bgra = new BgraColorFromArgb(c.R, c.G, c.B, c.A).Bgra;
            
        }

        public uint Bgra { get; }
    }

    public class HexColorFromBgra
    {
        public HexColorFromBgra(IBgraColor color)
        {
            var c = new MediaColorFromArgbColor(
                new RgbaColorFromBgra(color.Bgra)
            ).Color;

            System.ComponentModel.TypeConverter converter =
                System.ComponentModel.TypeDescriptor.GetConverter(
                    c
                );

            Hex = converter.ConvertToString(c);
        }

        public string Hex { get; }
    }

    public class BgraColorFromArgb : IBgraColor
    {
        private readonly uint _value;

        public BgraColorFromArgb(IArgbColor color) : this(color.R, color.G, color.B, color.A)
        {
        }

        public BgraColorFromArgb(byte r, byte g, byte b, byte a = 255)
        {
            _value = (uint) a << 24;
            _value |= (uint)r << 16;
            _value |= (uint)g << 8;
            _value |= (uint)b << 0;
        }

        public uint Bgra => _value;
    }

    public class ModifiedBgra : IBgraColor
    {
        public ModifiedBgra(uint origin, byte? r = null, byte? g = null, byte? b = null, byte? a = null)
        {
            var rgba = new RgbaColorFromBgra(origin);

            Bgra = new BgraColorFromArgb(
                r ?? rgba.R,
                g ?? rgba.G,
                b ?? rgba.B,
                a ?? rgba.A
            ).Bgra;
        }

        public uint Bgra { get; }
    }

    public interface IArgbColor
    {
        byte A {get;}
        byte R {get;}
        byte G {get;}
        byte B {get; }
    }

    public class RgbWithAlpha : IArgbColor
    {
        private readonly IArgbColor _color;
        private readonly byte _alpha;

        public RgbWithAlpha(IArgbColor color, byte alpha)
        {
            _color = color;
            _alpha = alpha;
        }

        public byte A => _alpha;

        public byte R => _color.R;

        public byte G => _color.G;

        public byte B => _color.B;
    }

    public class ComplementaryArgb : IArgbColor
    {
        private readonly IArgbColor _color;

        public ComplementaryArgb(IArgbColor color)
        {
            _color = color;
        }

        public byte A => _color.A;
        public byte R => (byte)(255 - _color.R);
        public byte G => (byte)(255 - _color.G);
        public byte B => (byte)(255 - _color.B);
    }

    public interface IMediaColor
    {
        System.Windows.Media.Color Color { get; }
    }

    public class MediaColorFromArgbColor : IMediaColor
    {
        private readonly IArgbColor _argb;

        public MediaColorFromArgbColor(IArgbColor argb)
        {
            _argb = argb;

            Color = System.Windows.Media.Color.FromArgb(argb.A, argb.R, argb.G, argb.B);
        }

        public System.Windows.Media.Color Color { get; }
    }

    public class ArgbColorFromAhsv : IArgbColor
    {
        private readonly IAhsvColor _ahsv;

        public ArgbColorFromAhsv(IAhsvColor ahsv)
        {
            _ahsv = ahsv;

            var s = ahsv.S * 100;
            var v = ahsv.V * 100;

            var vMin = (100f - s) * v/100f;
            var a = (v - vMin) * (ahsv.H % 60 / 60f);
            var vInc = vMin + a;
            var vDec = v - a;

            float rx = 0, gx = 0, bx = 0;

            if (ahsv.H >= 0 && ahsv.H < 60)
            {
                rx = v;
                gx = vInc;
                bx = vMin;
            }

            if (ahsv.H >= 60 && ahsv.H < 120)
            {
                rx = vDec;
                gx = v;
                bx = vMin;
            }

            if (ahsv.H >= 120 && ahsv.H < 180)
            {
                rx = vMin;
                gx = v;
                bx = vInc;
            }

            if (ahsv.H >= 180 && ahsv.H < 240)
            {
                rx = vMin;
                gx = vDec;
                bx = v;
            }

            if (ahsv.H >= 240 && ahsv.H < 300)
            {
                rx = vInc;
                gx = vMin;
                bx = v;
            }

            if (ahsv.H >= 300 && ahsv.H < 360)
            {
                rx = v;
                gx = vMin;
                bx = vDec;
            }

            R = (byte)Math.Round(((rx/100f) * 255f));
            G = (byte)Math.Round(((gx/100f) * 255f));
            B = (byte)Math.Round(((bx / 100f) * 255f));
        }

        public byte A => _ahsv.A;
        public byte R {get;}
        public byte G {get;}
        public byte B {get; }
    }

    public interface IAhsvColor
    {
        float H { get; }

        float S { get; }

        float V { get; }

        byte A { get; }
    }

    public class ModifiedAhsv : IAhsvColor
    {
        public ModifiedAhsv(IAhsvColor origin, float? h = null, float? s = null, float? v = null, byte? a = null)
        {
            A = a ?? origin.A;
            H = (h ?? origin.H) % 360;
            S = s ?? origin.S;
            V = v ?? origin.V;
        }

        public float H { get; }
        
        public float S { get; }
        
        public float V { get; }
        
        public byte A { get; }
    }

    public class AhsvColor : IAhsvColor
    {
        public AhsvColor(float h, float s, float v, byte a)
        {
            H = h;
            S = s;
            V = v;
            A = a;
        }

        public float H { get; }
        public float S { get; }
        public float V { get; }
        public byte A { get; }
    }

    public class AhsvColorFromRgba : IAhsvColor
    {
        public AhsvColorFromRgba(IArgbColor argb)
        {
            A = argb.A;

            var c = Color.FromArgb(argb.A, argb.R, argb.G, argb.B);

            H =  c.GetHue();

            var rx = argb.R / 255f;
            var gx = argb.G / 255f;
            var bx = argb.B / 255f;

            var min = Math.Min(rx, Math.Min(gx, bx));
            var max = Math.Max(rx, Math.Max(gx, bx));
            
            if (max < 0.0001f)
            {
                S = 0;
            }
            else
            {
                S = 1 - min / max;
            }

            V = max;
        }

        public float H { get; }

        public float S { get; }

        public float V { get; }
        
        public byte A { get; }
    }

    public class RgbaColorFromBgra : IArgbColor
    {
        private readonly uint _bgra;

        public RgbaColorFromBgra(uint bgra)
        {
            _bgra = bgra;
        }

        public byte A => new UintSegment(_bgra, segmentNumber: 3).Segment;
        public byte R => new UintSegment(_bgra, segmentNumber: 2).Segment;
        public byte G => new UintSegment(_bgra, segmentNumber: 1).Segment;
        public byte B => new UintSegment(_bgra, segmentNumber: 0).Segment;
    }

    public class UintSegment
    {
        private readonly uint _value;
        private readonly int _segmentNumber;

        public UintSegment(uint value, int segmentNumber)
        {
            _value = value;
            _segmentNumber = segmentNumber;
        }

        public byte Segment
        {
            get
            {
                var offset = 8 * _segmentNumber;
                var mask = (uint)255 << offset;
                var onlyA = _value & mask;
                var a = onlyA >> offset;

                return (byte)a;
            }
        }
    }
}