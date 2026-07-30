using SkiaSharp;
using Tonome.Framework.Animation;

namespace Tonome.Compositor.Effects;

public class DesktopSwitcher3D
{
    private readonly IReadOnlyList<Workspace.Workspace> _workspaces;
    private readonly SpringAnimation _scale;
    private readonly SpringAnimation _opacity;
    private readonly SpringAnimation _cardRotation;
    private int _selectedIndex;
    private bool _dismissed;

    public bool IsDismissed => _dismissed && _opacity.IsCompleted;

    public DesktopSwitcher3D(IReadOnlyList<Workspace.Workspace> workspaces)
    {
        _workspaces = workspaces;
        _scale = new SpringAnimation(0, 1) { Stiffness = 180, Damping = 14 };
        _opacity = new SpringAnimation(0, 1);
        _cardRotation = new SpringAnimation(0, 15);
    }

    public void SelectNext()
    {
        _selectedIndex = (_selectedIndex + 1) % _workspaces.Count;
    }

    public void SelectPrevious()
    {
        _selectedIndex = (_selectedIndex - 1 + _workspaces.Count) % _workspaces.Count;
    }

    public void Dismiss()
    {
        _opacity.Target = 0;
        _scale.Target = 0;
        _dismissed = true;
    }

    public void Render(SKCanvas canvas, double delta, int width, int height)
    {
        if (_workspaces.Count == 0) return;

        _scale.Update(delta);
        _opacity.Update(delta);
        _cardRotation.Update(delta);

        using var bgPaint = new SKPaint
        {
            Color = new SKColor(0, 0, 0, (byte)(160 * _opacity.Value))
        };
        canvas.DrawRect(0, 0, width, height, bgPaint);

        var centerX = width / 2f;
        var centerY = height / 2f;
        var cardW = 350 * _scale.Value;
        var cardH = 220 * _scale.Value;
        var spacing = 380 * _scale.Value;
        var totalW = _workspaces.Count * spacing;
        var startX = centerX - totalW / 2f;

        for (var i = 0; i < _workspaces.Count; i++)
        {
            var x = startX + i * spacing;
            var y = centerY;
            var isSelected = i == _selectedIndex;

            canvas.Save();

            var pivotX = x + cardW / 2f;
            var pivotY = y + cardH / 2f;

            canvas.Translate(pivotX, pivotY);
            canvas.RotateRadians(_cardRotation.Value * MathF.PI / 180f * (isSelected ? 0 : (i < _selectedIndex ? -1 : 1)));
            canvas.Translate(-pivotX, -pivotY);

            if (isSelected)
            {
                canvas.Scale(1.15f, 1.15f, pivotX, pivotY);
            }

            var alpha = (byte)((isSelected ? 1.0f : 0.6f) * _opacity.Value * 255);
            var cardColor = isSelected
                ? new SKColor(35, 35, 50, alpha)
                : new SKColor(25, 25, 35, alpha);

            using var cardPaint = new SKPaint
            {
                Color = cardColor,
                IsAntialias = true,
                MaskFilter = isSelected ? SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 10) : null
            };

            var rect = new SKRect(x, y, x + cardW, y + cardH);
            canvas.DrawRoundRect(rect, 16, 16, cardPaint);

            using var glassPaint = new SKPaint
            {
                Color = new SKColor(255, 255, 255, (byte)(isSelected ? 15 : 8)),
                IsAntialias = true
            };
            canvas.DrawRoundRect(rect, 16, 16, glassPaint);

            if (isSelected)
            {
                using var borderPaint = new SKPaint
                {
                    Color = new SKColor(0, 120, 212, alpha),
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = 2
                };
                canvas.DrawRoundRect(rect, 16, 16, borderPaint);
            }

            using var font = new SKFont(
                SKTypeface.FromFamilyName("Segoe UI", SKFontStyleWeight.SemiBold,
                    SKFontStyleWidth.Normal, SKFontStyleSlant.Upright), 16);
            using var textPaint = new SKPaint
            {
                Color = new SKColor(255, 255, 255, alpha),
                IsAntialias = true
            };
            canvas.DrawText(_workspaces[i].Name, x + 20, y + 30, SKTextAlign.Left, font, textPaint);

            var previewY = y + 45;
            foreach (var window in _workspaces[i].Windows.Take(4))
            {
                using var previewPaint = new SKPaint
                {
                    Color = new SKColor(60, 60, 80, (byte)(alpha / 2)),
                    IsAntialias = true
                };
                var previewRect = new SKRect(x + 15, previewY, x + cardW - 15, previewY + 35);
                canvas.DrawRoundRect(previewRect, 6, 6, previewPaint);

                using var titleFont = new SKFont(null, 10);
                using var titlePaint = new SKPaint
                {
                    Color = new SKColor(200, 200, 220, alpha),
                    IsAntialias = true
                };
                var title = window.Title;
                if (title.Length > 25) title = title[..22] + "...";
                canvas.DrawText(title, x + 22, previewY + 23, SKTextAlign.Left, titleFont, titlePaint);

                previewY += 40;
            }

            using var countFont = new SKFont(null, 11);
            using var countPaint = new SKPaint
            {
                Color = new SKColor(150, 150, 180, alpha),
                IsAntialias = true
            };
            canvas.DrawText($"{_workspaces[i].WindowCount} windows", x + 20, y + cardH - 12,
                SKTextAlign.Left, countFont, countPaint);

            canvas.Restore();
        }
    }
}
