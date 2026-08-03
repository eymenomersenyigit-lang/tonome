using Silk.NET.Core;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Tonome.Framework.Rendering;

namespace Tonome.Framework;

public class TonomeApplication
{
    private IWindow _window = null!;
    private TonomeRenderer? _renderer;
    private readonly int _width;
    private readonly int _height;
    private readonly string _title;

    public TonomeApplication(int width = 1920, int height = 1080, string title = "Tonome Desktop")
    {
        _width = width;
        _height = height;
        _title = title;
    }

    public IWindow Window => _window;
    public TonomeRenderer? Renderer => _renderer;

    public event Action? OnStarted;
    public event Action<double>? OnFrameUpdate;
    public event Action? OnShutdown;

    public void Run()
    {
        var platform = Silk.NET.Windowing.Window.GetWindowPlatform(false)
            ?? throw new Exception("No window platform available.");

        // Software renderers (llvmpipe) only support up to OpenGL 4.5 - fall back gracefully.
        Exception? lastError = null;
        foreach (var (major, minor) in new[] { (4, 6), (4, 5), (4, 3), (3, 3) })
        {
            try
            {
                Log($"Creating OpenGL {major}.{minor} Core window ({_width}x{_height})...");
                _window = platform.CreateWindow(BuildOptions(major, minor));
                lastError = null;
                break;
            }
            catch (Exception ex)
            {
                lastError = ex;
                Log($"OpenGL {major}.{minor} context creation failed: {ex.Message}");
            }
        }

        if (lastError is not null)
        {
            Log("FATAL: no usable OpenGL context could be created.");
            Log(lastError.ToString());
            throw lastError;
        }

        _window.Load += OnLoad;
        _window.Render += OnRender;
        _window.Update += OnUpdate;
        _window.Closing += OnClosing;

        try
        {
            _window.Run();
        }
        catch (Exception ex)
        {
            Log("Session loop crashed:");
            Log(ex.ToString());
            throw;
        }
    }

    private WindowOptions BuildOptions(int glMajor, int glMinor)
    {
        var options = WindowOptions.Default;
        options.Size = new Vector2D<int>(_width, _height);
        options.Title = _title;
        options.WindowBorder = WindowBorder.Fixed;
        options.API = new GraphicsAPI
        {
            API = ContextAPI.OpenGL,
            Profile = ContextProfile.Core,
            Version = new APIVersion(glMajor, glMinor)
        };
        options.ShouldSwapAutomatically = true;
        options.IsVisible = true;
        options.VSync = false;
        return options;
    }

    private void OnLoad()
    {
        _renderer = new TonomeRenderer(_window);
        Log($"Renderer initialized (GL {_glVersionInfo()})");
        OnStarted?.Invoke();
    }

    private string _glVersionInfo() =>
        Silk.NET.OpenGL.GL.GetApi(_window).GetStringS(StringName.Version);

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

    internal static void Log(string message)
    {
        Console.WriteLine($"[tonome] {message}");
        try
        {
            var home = Environment.GetEnvironmentVariable("HOME") ?? "/tmp";
            File.AppendAllText(Path.Combine(home, "tonome-session.log"), $"[{DateTime.Now:O}] {message}{Environment.NewLine}");
        }
        catch
        {
            // logging must never crash the session
        }
    }
}
