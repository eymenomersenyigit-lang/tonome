using SkiaSharp;
using Tonome.Framework.Controls;
using Tonome.Framework.Types;

namespace Tonome.Installer;

public class WelcomeWizard : Panel
{
    public int ScreenWidth { get; set; } = 1024;
    public int ScreenHeight { get; set; } = 720;

    private int _currentStep;
    private readonly List<WizardStep> _steps = new();

    public WelcomeWizard()
    {
        Width = ScreenWidth;
        Height = ScreenHeight;
        CornerRadius = 0;
    }

    public void AddStep(string title, string description, Action? onEnter = null, Action? onLeave = null)
    {
        _steps.Add(new WizardStep
        {
            Title = title,
            Description = description,
            OnEnter = onEnter,
            OnLeave = onLeave
        });
    }

    public void Next()
    {
        if (_currentStep < _steps.Count - 1)
        {
            _steps[_currentStep].OnLeave?.Invoke();
            _currentStep++;
            _steps[_currentStep].OnEnter?.Invoke();
        }
    }

    public void Previous()
    {
        if (_currentStep > 0)
        {
            _steps[_currentStep].OnLeave?.Invoke();
            _currentStep--;
            _steps[_currentStep].OnEnter?.Invoke();
        }
    }

    public override void Render(SKCanvas canvas, double delta)
    {
        using var bgPaint = new SKPaint
        {
            Color = new SKColor(15, 15, 25)
        };
        canvas.DrawRect(0, 0, Width, Height, bgPaint);

        var sideW = 240;
        using var sideBg = new SKPaint
        {
            Color = new SKColor(25, 25, 40, 200)
        };
        canvas.DrawRect(0, 0, sideW, Height, sideBg);

        for (var i = 0; i < _steps.Count; i++)
        {
            var sy = 80 + i * 56;
            var isActive = i == _currentStep;

            if (isActive)
            {
                using var activePaint = new SKPaint
                {
                    Color = new SKColor(0, 120, 212, 40)
                };
                var activeRect = new SKRect(0, sy - 8, sideW, sy + 40);
                canvas.DrawRect(activeRect, activePaint);
            }

            using var stepFont = new SKFont(null, 13);
            using var stepPaint = new SKPaint
            {
                Color = isActive ? SKColors.White : new SKColor(180, 180, 200, 150),
                IsAntialias = true
            };
            canvas.DrawText($"{i + 1}. {_steps[i].Title}", 24, sy + 22, SKTextAlign.Left, stepFont, stepPaint);
        }

        var contentX = sideW + 40;
        var contentY = 60;

        if (_currentStep < _steps.Count)
        {
            var step = _steps[_currentStep];

            using var titleFont = new SKFont(
                SKTypeface.FromFamilyName("Segoe UI", SKFontStyleWeight.SemiBold,
                    SKFontStyleWidth.Normal, SKFontStyleSlant.Upright), 24);
            using var titlePaint = new SKPaint
            {
                Color = SKColors.White,
                IsAntialias = true
            };
            canvas.DrawText(step.Title, contentX, contentY + 30, SKTextAlign.Left, titleFont, titlePaint);

            using var descFont = new SKFont(null, 14);
            using var descPaint = new SKPaint
            {
                Color = new SKColor(200, 200, 220, 200),
                IsAntialias = true
            };
            canvas.DrawText(step.Description, contentX, contentY + 70, SKTextAlign.Left, descFont, descPaint);
        }

        RenderNavigation(canvas, sideW);
    }

    private void RenderNavigation(SKCanvas canvas, int sideW)
    {
        var btnY = Height - 60;

        for (var i = 0; i < _steps.Count; i++)
        {
            using var dotPaint = new SKPaint
            {
                Color = i == _currentStep
                    ? new SKColor(0, 120, 212)
                    : new SKColor(255, 255, 255, 60),
                IsAntialias = true
            };
            var dx = sideW + 20 + i * 20;
            canvas.DrawCircle(dx, btnY + 8, 5, dotPaint);
        }

        if (_currentStep < _steps.Count - 1)
        {
            using var btnFont = new SKFont(null, 14);
            using var btnPaint = new SKPaint
            {
                Color = new SKColor(0, 120, 212, 220),
                IsAntialias = true
            };
            canvas.DrawText("Next →", Width - 100, btnY + 14, SKTextAlign.Left, btnFont, btnPaint);
        }
        else
        {
            using var btnFont = new SKFont(null, 14);
            using var btnPaint = new SKPaint
            {
                Color = new SKColor(0, 200, 100, 220),
                IsAntialias = true
            };
            canvas.DrawText("Install ✓", Width - 120, btnY + 14, SKTextAlign.Left, btnFont, btnPaint);
        }

        if (_currentStep > 0)
        {
            using var backFont = new SKFont(null, 14);
            using var backPaint = new SKPaint
            {
                Color = new SKColor(200, 200, 220, 150),
                IsAntialias = true
            };
            canvas.DrawText("← Back", sideW + 20, btnY + 14, SKTextAlign.Left, backFont, backPaint);
        }
    }
}

public class WizardStep
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public Action? OnEnter { get; set; }
    public Action? OnLeave { get; set; }
}
