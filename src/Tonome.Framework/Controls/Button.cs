using SkiaSharp;
using Tonome.Framework.Types;

namespace Tonome.Framework.Controls;

public class Button : Control
{
    public string Text { get; set; } = "";
    public Color HoverColor { get; set; } = new(60, 60, 60);
    public Color PressColor { get; set; } = new(40, 40, 40);
    public float TextSize { get; set; } = 14f;

    private bool _isHovered;
    private bool _isPressed;

    public event Action? OnClick;

    public Button()
    {
        BackgroundColor = new Color(45, 45, 45);
        Height = 36;
        Width = 120;
    }

    public override void Render(SKCanvas canvas, double delta)
    {
        if (!Visible) return;

        var bg = _isPressed ? PressColor :
                 _isHovered ? HoverColor : BackgroundColor;

        using var bgPaint = new SKPaint
        {
            Color = bg.ToSkia(),
            IsAntialias = true
        };
        var rect = new SKRect(AbsoluteX, AbsoluteY, AbsoluteX + Width, AbsoluteY + Height);
        canvas.DrawRoundRect(rect, CornerRadius, CornerRadius, bgPaint);

        if (!string.IsNullOrEmpty(Text))
        {
            using var font = new SKFont(null, TextSize);
            using var textPaint = new SKPaint
            {
                Color = ForegroundColor.ToSkia(),
                IsAntialias = true
            };
            var textX = AbsoluteX + Width / 2f;
            var textY = AbsoluteY + Height / 2f + TextSize / 3f;
            canvas.DrawText(Text, textX, textY, SKTextAlign.Center, font, textPaint);
        }
    }

    public override void OnMouseDown(int x, int y)
    {
        if (HitTest(x, y))
            _isPressed = true;
    }

    public override void OnMouseUp(int x, int y)
    {
        if (_isPressed && HitTest(x, y))
            OnClick?.Invoke();
        _isPressed = false;
    }

    public override void OnMouseMove(int x, int y)
    {
        _isHovered = HitTest(x, y);
    }
}
