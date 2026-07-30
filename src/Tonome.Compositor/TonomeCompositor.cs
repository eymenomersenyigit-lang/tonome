using Tonome.Compositor.Wayland;
using Tonome.Compositor.Workspace;
using Tonome.Compositor.Effects;
using Tonome.Compositor.Input;
using Tonome.Compositor.Output;
using Tonome.Framework;
using Tonome.Framework.Rendering;

namespace Tonome.Compositor;

public class TonomeCompositor : IDisposable
{
    private readonly WaylandServer _wayland;
    private readonly WindowManager _windowManager;
    private readonly WorkspaceManager _workspaceManager;
    private readonly CompositorEffects _effects;
    private readonly CompositorInput _input;
    private readonly OutputManager _output;
    private readonly TonomeRenderer _renderer;
    private readonly TonomeApplication _app;

    public bool Running { get; private set; }

    public TonomeCompositor(TonomeApplication app)
    {
        _app = app;
        _renderer = app.Renderer!;

        _wayland = new WaylandServer();
        _windowManager = new WindowManager();
        _workspaceManager = new WorkspaceManager();
        _effects = new CompositorEffects();
        _input = new CompositorInput(_windowManager);
        _output = new OutputManager();

        _windowManager.OnWindowStateChanged += OnWindowStateChanged;
        _input.OnAltTab += OnAltTab;
        _input.OnSuperTab += OnSuperTab;
        _input.OnSuperR += OnSuperR;

        _renderer.OnRender += RenderFrame;
    }

    public void Start()
    {
        Running = true;
        _wayland.Start();
        _workspaceManager.SetActiveWorkspace(0);

        _output.DetectOutputs();
        var primary = _output.PrimaryOutput;
        _windowManager.SetupDefaultLayout(primary?.Size ?? new Framework.Types.Size(1920, 1080));
    }

    public void Stop()
    {
        Running = false;
        _wayland.Stop();
    }

    private void RenderFrame(SkiaSharp.SKCanvas canvas, double delta, int width, int height)
    {
        if (!Running) return;

        var workspace = _workspaceManager.ActiveWorkspace;
        if (workspace == null) return;

        workspace.RenderBackground(canvas, delta);

        foreach (var window in workspace.Windows)
        {
            if (_windowManager.AnimatingWindows.TryGetValue(window.Id, out var anim))
            {
                anim.Apply(canvas, delta);
            }
            window.Render(canvas, delta);
        }

        _effects.RenderOverlays(canvas, delta, width, height);
    }

    private void OnWindowStateChanged(CompositorWindow window, WindowState oldState, WindowState newState)
    {
        _effects.AnimateWindowState(window, oldState, newState);
    }

    private void OnAltTab()
    {
        _effects.ShowAltTabCarousel(_windowManager.GetWindowList(), _workspaceManager);
    }

    private void OnSuperTab()
    {
        _effects.ShowDesktopSwitcher(_workspaceManager.Workspaces);
    }

    private void OnSuperR()
    {
        _effects.ShowRunDialog();
    }

    public void Dispose()
    {
        _wayland?.Dispose();
        _windowManager?.Dispose();
        _workspaceManager?.Dispose();
    }
}
