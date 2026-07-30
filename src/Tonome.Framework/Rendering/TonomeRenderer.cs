using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using SkiaSharp;

namespace Tonome.Framework.Rendering;

public class TonomeRenderer : IDisposable
{
    private readonly IWindow _window;
    private GL _gl = null!;
    private GRContext _skiaContext = null!;
    private SKSurface _skiaSurface = null!;
    private GRBackendRenderTarget _renderTarget = default!;

    private int _fbWidth;
    private int _fbHeight;

    public float RenderScale { get; set; } = 2.0f;
    public bool EnableSSAA { get; set; } = true;

    public TonomeRenderer(IWindow window)
    {
        _window = window;
        _gl = GL.GetApi(window);
        InitializeSkia();
    }

    private void InitializeSkia()
    {
        _fbWidth = (int)(_window.FramebufferSize.X / RenderScale);
        _fbHeight = (int)(_window.FramebufferSize.Y / RenderScale);

        var glInterface = GRGlInterface.Create();
        _skiaContext = GRContext.CreateGl(glInterface);

        var glInfo = new GRGlFramebufferInfo(0, GetSkiaColorType().ToGlSizedFormat());
        _renderTarget = new GRBackendRenderTarget(
            _fbWidth, _fbHeight, 0, 8, glInfo);

        _skiaSurface = SKSurface.Create(
            _skiaContext, _renderTarget,
            GRSurfaceOrigin.BottomLeft, GetSkiaColorType());
    }

    private static SKColorType GetSkiaColorType() => SKColorType.Rgba8888;

    public void Render(double delta)
    {
        var canvas = _skiaSurface.Canvas;

        canvas.Clear(SKColors.Transparent);

        canvas.Save();
        if (EnableSSAA)
            canvas.Scale(1f / RenderScale, 1f / RenderScale);

        OnRender?.Invoke(canvas, delta, _fbWidth, _fbHeight);

        canvas.Restore();

        canvas.Flush();
        _skiaContext.Flush();

        _gl.Viewport(0, 0, (uint)_window.FramebufferSize.X, (uint)_window.FramebufferSize.Y);
    }

    public Action<SKCanvas, double, int, int>? OnRender { get; set; }

    public void Resize(int width, int height)
    {
        _skiaSurface?.Dispose();
        _renderTarget?.Dispose();

        InitializeSkia();
    }

    public void Dispose()
    {
        _skiaSurface?.Dispose();
        _renderTarget?.Dispose();
        _skiaContext?.Dispose();
        _gl?.Dispose();
    }
}
