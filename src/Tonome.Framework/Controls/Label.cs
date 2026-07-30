using SkiaSharp;
using Tonome.Framework.Types;

namespace Tonome.Framework.Controls;

public class Label : Control
{
    public string Text { get; set; } = "";
    public float TextSize { get; set; } = 14f;
    public bool Bold { get; set; }
    public bool Center { get; set; }

    public Label()
    {
        Height = 24;
    }

    public override void Render(SKCanvas canvas, double delta)
    {
        if (!Visible || string.IsNullOrEmpty(Text)) return;

        var typeface = Bold
            ? SKTypeface.FromFamilyName("Segoe UI", SKFontStyleWeight.Bold,
                SKFontStyleWidth.Normal, SKFontStyleSlant.Upright)
            : null;

        using var font = new SKFont(typeface, TextSize);
        using var paint = new SKPaint
        {
            Color = ForegroundColor.ToSkia(),
            IsAntialias = true
        };

        var x = Center ? AbsoluteX + Width / 2f : AbsoluteX;
        var y = AbsoluteY + TextSize;

        var text = Text;
        if (Width > 0 && !Center)
        {
            var maxWidth = Width - Margin.Left - Margin.Right;
            if (font.MeasureText(Text) > maxWidth)
            {
                text = TruncateText(font, Text, maxWidth);
            }
        }

        canvas.DrawText(text, x, y, Center ? SKTextAlign.Center : SKTextAlign.Left, font, paint);
    }

    private static string TruncateText(SKFont font, string text, float maxWidth)
    {
        for (var i = text.Length - 1; i > 0; i--)
        {
            var truncated = text[..i] + "...";
            if (font.MeasureText(truncated) <= maxWidth)
                return truncated;
        }
        return "...";
    }
}
