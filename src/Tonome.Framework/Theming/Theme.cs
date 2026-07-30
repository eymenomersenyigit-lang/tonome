using Tonome.Framework.Types;

namespace Tonome.Framework.Theming;

public class Theme
{
    public string Name { get; set; } = "Default Glass";
    public bool GlassEnabled { get; set; } = true;
    public int CornerRadius { get; set; } = 12;
    public float GlassBlurSigma { get; set; } = 10f;

    public Color BackgroundColor { get; set; } = new(20, 20, 20);
    public Color SurfaceColor { get; set; } = new(255, 255, 255, 25);
    public Color AccentColor { get; set; } = new(0, 120, 212);
    public Color TextColor { get; set; } = new(255, 255, 255);
    public Color TextSecondaryColor { get; set; } = new(200, 200, 200);
    public Color BorderColor { get; set; } = new(255, 255, 255, 30);

    public string FontFamily { get; set; } = "Segoe UI";
    public float FontSizeSmall { get; set; } = 12f;
    public float FontSizeNormal { get; set; } = 14f;
    public float FontSizeLarge { get; set; } = 18f;
    public float FontSizeTitle { get; set; } = 24f;

    public int DashIconSize { get; set; } = 48;
    public int DashPadding { get; set; } = 12;
    public int DashCornerRadius { get; set; } = 16;

    public int PanelHeight { get; set; } = 44;
    public int TitleBarHeight { get; set; } = 40;

    public static Theme Default => new();
}
