using SkiaSharp;
using Tonome.Framework.Types;

namespace Tonome.Framework.Controls;

public class Dash : Panel
{
    public int IconSize { get; set; } = 48;
    public int Spacing { get; set; } = 8;
    public int Padding { get; set; } = 12;
    public bool AutoHide { get; set; }
    public int ScreenWidth { get; set; } = 1920;

    public Dash()
    {
        Height = IconSize + Padding * 2;
        CornerRadius = 16;
    }

    public void AddAppIcon(string name)
    {
        var icon = new DashIcon
        {
            Text = name,
            Width = IconSize,
            Height = IconSize,
            CornerRadius = 14,
            BackgroundColor = new Color(255, 255, 255, 30)
        };
        AddChild(icon);
        ReflowIcons();
    }

    private void ReflowIcons()
    {
        var totalWidth = Children.Count * (IconSize + Spacing) - Spacing;
        X = (ScreenWidth - totalWidth) / 2;
        Width = totalWidth + Padding * 2;

        for (var i = 0; i < Children.Count; i++)
        {
            Children[i].X = Padding + i * (IconSize + Spacing);
            Children[i].Y = Padding;
        }
    }

    public override void Render(SKCanvas canvas, double delta)
    {
        DrawGlassBackground(canvas, 8f);
        foreach (var child in Children)
            if (child.Visible)
                child.Render(canvas, delta);
    }
}

public class DashIcon : Control
{
    public string Text { get; set; } = "";

    public DashIcon()
    {
        CornerRadius = 14;
    }

    public override void Render(SKCanvas canvas, double delta)
    {
        DrawBackground(canvas);

        if (!string.IsNullOrEmpty(Text))
        {
            using var font = new SKFont(null, 11);
            using var paint = new SKPaint
            {
                Color = new SKColor(255, 255, 255, 200),
                IsAntialias = true
            };
            var letter = Text.Length > 0 ? Text[..1].ToUpper() : "?";
            canvas.DrawText(letter, AbsoluteX + Width / 2f, AbsoluteY + Height / 2f + 4, SKTextAlign.Center, font, paint);
        }
    }
}
