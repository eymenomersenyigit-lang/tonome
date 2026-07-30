using Tonome.Framework.Controls;
using Tonome.Framework.Types;

namespace Tonome.Compositor;

public enum WindowState
{
    Normal,
    Minimized,
    Maximized,
    Fullscreen,
    Closing
}

public class CompositorWindow
{
    private static int _nextId;

    public int Id { get; }
    public Window FrameworkWindow { get; }
    public WindowState State { get; private set; } = WindowState.Normal;
    public Size RestoredSize { get; set; }
    public Point RestoredPosition { get; set; }
    public bool IsFocused { get; set; }
    public string Title => FrameworkWindow.Title;
    public bool IsUrgent { get; set; }

    public CompositorWindow(Window frameworkWindow)
    {
        Id = Interlocked.Increment(ref _nextId);
        FrameworkWindow = frameworkWindow;
        RestoredSize = new Size(frameworkWindow.Width, frameworkWindow.Height);
        RestoredPosition = new Point(frameworkWindow.X, frameworkWindow.Y);
    }

    public void SetState(WindowState newState)
    {
        var old = State;
        State = newState;
        StateChanged?.Invoke(this, old, newState);
    }

    public void Render(SkiaSharp.SKCanvas canvas, double delta)
    {
        FrameworkWindow.Render(canvas, delta);
    }

    public event Action<CompositorWindow, WindowState, WindowState>? StateChanged;
}

public class WindowManager : IDisposable
{
    private readonly Dictionary<int, CompositorWindow> _windows = new();
    private readonly Dictionary<int, WindowAnimation> _animatingWindows = new();
    private CompositorWindow? _focusedWindow;
    private int _focusStackIndex;

    public IReadOnlyDictionary<int, CompositorWindow> Windows => _windows;
    public IReadOnlyDictionary<int, WindowAnimation> AnimatingWindows => _animatingWindows;
    public CompositorWindow? FocusedWindow => _focusedWindow;

    public event Action<CompositorWindow, WindowState, WindowState>? OnWindowStateChanged;

    public void SetupDefaultLayout(Framework.Types.Size outputSize)
    {
        _focusedWindow = null;
    }

    public int AddWindow(Window frameworkWindow)
    {
        var cw = new CompositorWindow(frameworkWindow);
        cw.StateChanged += (w, o, n) => OnWindowStateChanged?.Invoke(w, o, n);
        _windows[cw.Id] = cw;
        FocusWindow(cw.Id);
        return cw.Id;
    }

    public void RemoveWindow(int windowId)
    {
        if (_windows.TryGetValue(windowId, out var cw))
        {
            cw.SetState(WindowState.Closing);
            _animatingWindows.Remove(windowId);
            _windows.Remove(windowId);

            if (_focusedWindow?.Id == windowId)
            {
                FocusNextWindow();
            }
        }
    }

    public bool FocusWindow(int windowId)
    {
        if (!_windows.TryGetValue(windowId, out var cw)) return false;
        if (_focusedWindow != null)
            _focusedWindow.IsFocused = false;
        _focusedWindow = cw;
        cw.IsFocused = true;
        cw.IsUrgent = false;
        _focusStackIndex++;
        return true;
    }

    public void FocusNextWindow()
    {
        var sorted = _windows.Values
            .OrderByDescending(w => w.IsUrgent)
            .ThenByDescending(w => w.IsFocused)
            .ToList();

        if (sorted.Count == 0) return;

        var currentIdx = sorted.FindIndex(w => w.Id == _focusedWindow?.Id);
        var nextIdx = (currentIdx + 1) % sorted.Count;
        FocusWindow(sorted[nextIdx].Id);
    }

    public List<CompositorWindow> GetWindowList()
    {
        return _windows.Values
            .OrderByDescending(w => w == _focusedWindow)
            .ThenByDescending(w => w.IsUrgent)
            .ToList();
    }

    public CompositorWindow? HitTest(int x, int y)
    {
        foreach (var window in _windows.Values.Reverse())
        {
            var fw = window.FrameworkWindow;
            if (x >= fw.X && x <= fw.X + fw.Width &&
                y >= fw.Y && y <= fw.Y + fw.Height)
                return window;
        }
        return null;
    }

    public TiledLayout CalculateTiledLayout(Framework.Types.Size area)
    {
        var visible = _windows.Values
            .Where(w => w.State != WindowState.Minimized)
            .ToList();

        if (visible.Count == 0)
            return new TiledLayout();

        return new TiledLayout
        {
            Windows = visible,
            Layout = CalculateBestLayout(visible.Count, area)
        };
    }

    private static List<Framework.Types.Size> CalculateBestLayout(int count, Framework.Types.Size area)
    {
        var layouts = new List<Framework.Types.Size>();
        if (count == 1)
        {
            layouts.Add(area);
        }
        else if (count == 2)
        {
            layouts.Add(new Framework.Types.Size(area.Width / 2, area.Height));
            layouts.Add(new Framework.Types.Size(area.Width / 2, area.Height));
        }
        else
        {
            var cols = (int)Math.Ceiling(Math.Sqrt(count));
            var rows = (int)Math.Ceiling((double)count / cols);
            for (var i = 0; i < count; i++)
            {
                layouts.Add(new Framework.Types.Size(area.Width / cols, area.Height / rows));
            }
        }
        return layouts;
    }

    public void Dispose()
    {
        _windows.Clear();
        _animatingWindows.Clear();
    }
}

public class TiledLayout
{
    public List<CompositorWindow> Windows { get; set; } = new();
    public List<Framework.Types.Size> Layout { get; set; } = new();
}

public class WindowAnimation
{
    public int WindowId { get; set; }
    public Framework.Animation.SpringAnimation ScaleX { get; set; } = new(1, 1);
    public Framework.Animation.SpringAnimation ScaleY { get; set; } = new(1, 1);
    public Framework.Animation.SpringAnimation Opacity { get; set; } = new(1, 1);
    public Framework.Animation.SpringAnimation TranslateX { get; set; } = new(0, 0);
    public Framework.Animation.SpringAnimation TranslateY { get; set; } = new(0, 0);
    public Framework.Animation.SpringAnimation CornerRadius { get; set; } = new(12, 12);

    public void Apply(SkiaSharp.SKCanvas canvas, double delta)
    {
        ScaleX.Update(delta);
        ScaleY.Update(delta);
        Opacity.Update(delta);
        TranslateX.Update(delta);
        TranslateY.Update(delta);
        CornerRadius.Update(delta);
    }
}
