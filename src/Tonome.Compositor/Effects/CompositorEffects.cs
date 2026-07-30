using SkiaSharp;
using Tonome.Compositor.Workspace;
using Tonome.Framework.Animation;

namespace Tonome.Compositor.Effects;

public class CompositorEffects
{
    private readonly AnimationManager _animManager = new();
    private Carousel3D? _carousel;
    private DesktopSwitcher3D? _desktopSwitcher;
    private bool _showRunDialog;

    public void AnimateWindowState(CompositorWindow window, WindowState oldState, WindowState newState)
    {
        var anim = new WindowAnimation { WindowId = window.Id };

        switch (newState)
        {
            case WindowState.Minimized:
                anim.ScaleX = new SpringAnimation(1, 0) { Stiffness = 200, Damping = 15 };
                anim.ScaleY = new SpringAnimation(1, 0) { Stiffness = 200, Damping = 15 };
                anim.Opacity = new SpringAnimation(1, 0);
                break;
            case WindowState.Maximized:
                anim.ScaleX = new SpringAnimation(1, 1.05f) { Stiffness = 150, Damping = 10 };
                anim.ScaleY = new SpringAnimation(1, 1.05f);
                break;
            case WindowState.Closing:
                anim.ScaleX = new SpringAnimation(1, 0);
                anim.ScaleY = new SpringAnimation(1, 0);
                anim.Opacity = new SpringAnimation(1, 0);
                break;
            default:
                anim.ScaleX = new SpringAnimation(0, 1);
                anim.ScaleY = new SpringAnimation(0, 1);
                break;
        }
    }

    public void ShowAltTabCarousel(List<CompositorWindow> windows, WorkspaceManager workspaceManager)
    {
        _carousel = new Carousel3D(windows);
    }

    public void ShowDesktopSwitcher(IReadOnlyList<Workspace.Workspace> workspaces)
    {
        _desktopSwitcher = new DesktopSwitcher3D(workspaces);
    }

    public void ShowRunDialog()
    {
        _showRunDialog = true;
    }

    public void HideRunDialog()
    {
        _showRunDialog = false;
    }

    public void RenderOverlays(SKCanvas canvas, double delta, int width, int height)
    {
        _carousel?.Render(canvas, delta, width, height);
        _desktopSwitcher?.Render(canvas, delta, width, height);

        if (_showRunDialog)
            RenderRunDialog(canvas, width, height);

        if (_carousel?.IsDismissed == true)
            _carousel = null;
        if (_desktopSwitcher?.IsDismissed == true)
            _desktopSwitcher = null;
    }

    private static void RenderRunDialog(SKCanvas canvas, int width, int height)
    {
        using var bgPaint = new SKPaint
        {
            Color = new SKColor(0, 0, 0, 180)
        };
        canvas.DrawRect(0, 0, width, height, bgPaint);

        var dialogW = 500;
        var dialogH = 200;
        var dx = (width - dialogW) / 2;
        var dy = (height - dialogH) / 2;

        using var glassPaint = new SKPaint
        {
            Color = new SKColor(30, 30, 40, 230),
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 8),
            IsAntialias = true
        };
        var rect = new SKRect(dx, dy, dx + dialogW, dy + dialogH);
        canvas.DrawRoundRect(rect, 16, 16, glassPaint);

        using var borderPaint = new SKPaint
        {
            Color = new SKColor(255, 255, 255, 30),
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1
        };
        canvas.DrawRoundRect(rect, 16, 16, borderPaint);

        using var font = new SKFont(
            SKTypeface.FromFamilyName("Segoe UI", SKFontStyleWeight.SemiBold,
                SKFontStyleWidth.Normal, SKFontStyleSlant.Upright), 16);
        using var textPaint = new SKPaint
        {
            Color = SKColors.White,
            IsAntialias = true
        };
        canvas.DrawText("Run Command", dx + 20, dy + 35, SKTextAlign.Left, font, textPaint);

        using var inputBg = new SKPaint
        {
            Color = new SKColor(255, 255, 255, 20),
            IsAntialias = true
        };
        var inputRect = new SKRect(dx + 20, dy + 50, dx + dialogW - 20, dy + 90);
        canvas.DrawRoundRect(inputRect, 8, 8, inputBg);

        using var hintFont = new SKFont(null, 13);
        using var hintPaint = new SKPaint
        {
            Color = new SKColor(255, 255, 255, 100),
            IsAntialias = true
        };
        canvas.DrawText("Type a command and press Enter...", dx + 30, dy + 74, SKTextAlign.Left, hintFont, hintPaint);

        using var tipFont = new SKFont(null, 11);
        using var tipPaint = new SKPaint
        {
            Color = new SKColor(255, 255, 255, 80),
            IsAntialias = true
        };
        canvas.DrawText("Super+R to open  |  Esc to close", dx + 20, dy + 120, SKTextAlign.Left, tipFont, tipPaint);
    }
}
