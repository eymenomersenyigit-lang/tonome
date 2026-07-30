using SkiaSharp;

namespace Tonome.Framework.Controls;

public class Window : Panel
{
    public string Title { get; set; } = "Tonome Window";
    public bool IsDraggable { get; set; } = true;
    public bool ShowTitleBar { get; set; } = true;
    public int TitleBarHeight { get; set; } = 40;
    public bool GlassEnabled { get; set; } = true;

    private bool _isDragging;
    private int _dragStartX, _dragStartY;

    public override void Render(SKCanvas canvas, double delta)
    {
        if (!Visible) return;

        if (GlassEnabled)
            DrawGlassBackground(canvas);
        else
            DrawBackground(canvas);

        if (ShowTitleBar)
        {
            using var font = new SKFont(
                SKTypeface.FromFamilyName("Segoe UI", SKFontStyleWeight.SemiBold,
                    SKFontStyleWidth.Normal, SKFontStyleSlant.Upright), 14);
            using var titlePaint = new SKPaint
            {
                Color = new SKColor(255, 255, 255, 220),
                IsAntialias = true
            };
            canvas.DrawText(Title, AbsoluteX + 16, AbsoluteY + 26, SKTextAlign.Left, font, titlePaint);
        }

        foreach (var child in Children)
        {
            if (child.Visible)
                child.Render(canvas, delta);
        }
    }

    public override void OnMouseDown(int x, int y)
    {
        if (IsDraggable && y - AbsoluteY < TitleBarHeight)
        {
            _isDragging = true;
            _dragStartX = x;
            _dragStartY = y;
        }
    }

    public override void OnMouseMove(int x, int y)
    {
        if (_isDragging)
        {
            X += x - _dragStartX;
            Y += y - _dragStartY;
            _dragStartX = x;
            _dragStartY = y;
        }
    }

    public override void OnMouseUp(int x, int y)
    {
        _isDragging = false;
    }
}
