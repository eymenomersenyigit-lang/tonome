namespace Tonome.Framework.Types;

public struct Thickness
{
    public int Left { get; set; }
    public int Top { get; set; }
    public int Right { get; set; }
    public int Bottom { get; set; }

    public Thickness(int all)
    {
        Left = Top = Right = Bottom = all;
    }

    public Thickness(int left, int top, int right, int bottom)
    {
        Left = left; Top = top; Right = right; Bottom = bottom;
    }

    public static Thickness Zero => new(0);
}
