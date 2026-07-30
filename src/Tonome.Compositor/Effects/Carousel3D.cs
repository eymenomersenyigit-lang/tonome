using SkiaSharp;
using Tonome.Framework.Animation;

namespace Tonome.Compositor.Effects;

public class Carousel3D
{
    private readonly List<CompositorWindow> _windows;
    private readonly SpringAnimation _rotation;
    private readonly SpringAnimation _scale;
    private readonly SpringAnimation _opacity;
    private int _selectedIndex;
    private bool _dismissed;
    private bool _active;

    public bool IsDismissed => _dismissed && _opacity.IsCompleted;

    public Carousel3D(List<CompositorWindow> windows)
    {
        _windows = windows;
        _rotation = new SpringAnimation(0, 0);
        _scale = new SpringAnimation(0, 1);
        _opacity = new SpringAnimation(0, 1);
        _active = windows.Count > 0;
    }

    public void SelectNext()
    {
        if (_windows.Count == 0) return;
        _selectedIndex = (_selectedIndex + 1) % _windows.Count;
        _rotation.Target = -_selectedIndex * (360f / _windows.Count);
    }

    public void SelectPrevious()
    {
        if (_windows.Count == 0) return;
        _selectedIndex = (_selectedIndex - 1 + _windows.Count) % _windows.Count;
        _rotation.Target = -_selectedIndex * (360f / _windows.Count);
    }

    public void Dismiss()
    {
        _opacity.Target = 0;
        _scale.Target = 0;
        _dismissed = true;
    }

    public void Render(SKCanvas canvas, double delta, int width, int height)
    {
        if (!_active || _windows.Count == 0) return;

        _rotation.Update(delta);
        _scale.Update(delta);
        _opacity.Update(delta);

        using var bgPaint = new SKPaint
        {
            Color = new SKColor(0, 0, 0, (byte)(120 * _opacity.Value))
        };
        canvas.DrawRect(0, 0, width, height, bgPaint);

        var centerX = width / 2f;
        var centerY = height / 2f;
        var radius = Math.Min(width, height) * 0.3f;
        var totalCards = _windows.Count;

        for (var i = 0; i < totalCards; i++)
        {
            var angle = (i * (360f / totalCards) + _rotation.Value) * MathF.PI / 180f;
            var cardX = centerX + MathF.Sin(angle) * radius * _scale.Value;
            var cardY = centerY - MathF.Cos(angle) * radius * 0.3f * _scale.Value;
            var depth = (MathF.Cos(angle) + 1) / 2f;

            var cardW = 300 * _scale.Value;
            var cardH = 200 * _scale.Value;
            var alpha = (byte)((0.4f + depth * 0.6f) * _opacity.Value * 255);

            var isSelected = i == _selectedIndex;

            using var cardPaint = new SKPaint
            {
                Color = new SKColor(40, 40, 50, alpha),
                IsAntialias = true,
                MaskFilter = isSelected ? SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 8) : null
            };

            var rect = new SKRect(
                cardX - cardW / 2, cardY - cardH / 2,
                cardX + cardW / 2, cardY + cardH / 2);

            canvas.DrawRoundRect(rect, 12, 12, cardPaint);

            if (isSelected)
            {
                using var borderPaint = new SKPaint
                {
                    Color = new SKColor(0, 120, 212, alpha),
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = 2
                };
                canvas.DrawRoundRect(rect, 12, 12, borderPaint);
            }

            if (i < _windows.Count)
            {
                using var font = new SKFont(null, 13);
                using var titlePaint = new SKPaint
                {
                    Color = new SKColor(255, 255, 255, alpha),
                    IsAntialias = true
                };
                var title = _windows[i].Title;
                if (title.Length > 20)
                    title = title[..17] + "...";
                canvas.DrawText(title, cardX, cardY + cardH / 2 - 8, SKTextAlign.Center, font, titlePaint);
            }
        }
    }
}
