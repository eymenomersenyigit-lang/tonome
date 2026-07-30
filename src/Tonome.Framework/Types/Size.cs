namespace Tonome.Framework.Types;

public struct Size
{
    public int Width { get; set; }
    public int Height { get; set; }

    public Size(int width, int height)
    {
        Width = width;
        Height = height;
    }

    public static Size Zero => new(0, 0);
}
