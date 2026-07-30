using Silk.NET.Core;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using Tonome.Framework.Rendering;

namespace Tonome.Framework;

public class TonomeApplication
{
    private IWindow _window = null!;
    private TonomeRenderer? _renderer;
    private readonly WindowOptions _options;

    public TonomeApplication(int width = 1920, int height = 1080, string title = "Tonome Desktop")
    {
        _options = WindowOptions.Default;
        _options.Size = new Vector2D<int>(width, height);
        _options.Title = title;
        _options.WindowBorder = WindowBorder.Fixed;
        _options.API = new GraphicsAPI
        {
            API = ContextAPI.OpenGL,
            Profile = ContextProfile.Core,
            Version = new APIVersion(4, 6)
        };
    }

    public IWindow Window => _window;
    public TonomeRenderer? Renderer => _renderer;

    public event Action? OnStarted;
    public event Action<double>? OnFrameUpdate;
    public event Action? OnShutdown;

    public void Run()
    {
        var platform = Silk.NET.Windowing.Window.GetWindowPlatform(false) ?? throw new Exception("No window platform available.");
        _window = platform.CreateWindow(_options);

        _window.Load += OnLoad;
        _window.Render += OnRender;
        _window.Update += OnUpdate;
        _window.Closing += OnClosing;

        _window.Run();
    }

    private void OnLoad()
    {
        _renderer = new TonomeRenderer(_window);
        OnStarted?.Invoke();
    }

    private void OnRender(double delta)
    {
        _renderer?.Render(delta);
    }

    private void OnUpdate(double delta)
    {
        OnFrameUpdate?.Invoke(delta);
    }

    private void OnClosing()
    {
        _renderer?.Dispose();
        OnShutdown?.Invoke();
    }
}
