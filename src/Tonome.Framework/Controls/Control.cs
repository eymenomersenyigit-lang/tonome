using SkiaSharp;
using Tonome.Framework.Types;

namespace Tonome.Framework.Controls;

public abstract class Control
{
    public string Name { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public bool Visible { get; set; } = true;
    public bool Enabled { get; set; } = true;
    public float Opacity { get; set; } = 1.0f;
    public int CornerRadius { get; set; } = 12;
    public Color BackgroundColor { get; set; } = Color.Transparent;
    public Color ForegroundColor { get; set; } = Color.White;
    public Thickness Margin { get; set; } = Thickness.Zero;
    public Control? Parent { get; set; }

    public int AbsoluteX => Parent?.AbsoluteX + X ?? X;
    public int AbsoluteY => Parent?.AbsoluteY + Y ?? Y;

    protected Control()
    {
        Name = GetType().Name;
    }

    public abstract void Render(SKCanvas canvas, double delta);

    public virtual bool HitTest(int px, int py)
    {
        var ax = AbsoluteX;
        var ay = AbsoluteY;
        return px >= ax && px <= ax + Width &&
               py >= ay && py <= ay + Height;
    }

    public virtual void OnMouseDown(int x, int y) { }
    public virtual void OnMouseUp(int x, int y) { }
    public virtual void OnMouseMove(int x, int y) { }
    public virtual void OnKeyDown(string key) { }

    protected void DrawBackground(SKCanvas canvas)
    {
        if (BackgroundColor.A == 0) return;
        using var paint = new SKPaint
        {
            Color = BackgroundColor.ToSkia(),
            IsAntialias = true,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 2)
        };
        var rect = new SKRect(AbsoluteX, AbsoluteY, AbsoluteX + Width, AbsoluteY + Height);
        if (CornerRadius > 0)
            canvas.DrawRoundRect(rect, CornerRadius, CornerRadius, paint);
        else
            canvas.DrawRect(rect, paint);
    }

    protected void DrawGlassBackground(SKCanvas canvas, float blurSigma = 10f)
    {
        using var paint = new SKPaint
        {
            Color = new SKColor(255, 255, 255, 30),
            IsAntialias = true,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, blurSigma)
        };
        using var borderPaint = new SKPaint
        {
            Color = new SKColor(255, 255, 255, 40),
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1
        };
        var rect = new SKRect(AbsoluteX, AbsoluteY, AbsoluteX + Width, AbsoluteY + Height);
        canvas.DrawRoundRect(rect, CornerRadius, CornerRadius, paint);
        canvas.DrawRoundRect(rect, CornerRadius, CornerRadius, borderPaint);
    }
}
