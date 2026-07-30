using SkiaSharp;

namespace Tonome.Framework.Types;

public struct Color
{
    public byte R { get; set; }
    public byte G { get; set; }
    public byte B { get; set; }
    public byte A { get; set; }

    public Color(byte r, byte g, byte b, byte a = 255)
    {
        R = r; G = g; B = b; A = a;
    }

    public static Color Transparent => new(0, 0, 0, 0);
    public static Color White => new(255, 255, 255);
    public static Color Black => new(0, 0, 0);
    public static Color Accent => new(0, 120, 212);

    public SKColor ToSkia() => new(R, G, B, A);
}
