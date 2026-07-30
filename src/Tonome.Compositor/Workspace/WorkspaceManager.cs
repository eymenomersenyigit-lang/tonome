using Tonome.Framework.Animation;

namespace Tonome.Compositor.Workspace;

public class WorkspaceManager : IDisposable
{
    private readonly List<Workspace> _workspaces = new();
    private int _activeIndex;
    private readonly SpringAnimation _switchAnimation;

    public WorkspaceManager(int count = 4)
    {
        for (var i = 0; i < count; i++)
        {
            _workspaces.Add(new Workspace(i, $"Desktop {i + 1}"));
        }
        _switchAnimation = new SpringAnimation(0, 0);
    }

    public IReadOnlyList<Workspace> Workspaces => _workspaces;
    public Workspace? ActiveWorkspace => _activeIndex >= 0 && _activeIndex < _workspaces.Count
        ? _workspaces[_activeIndex] : null;
    public int ActiveIndex => _activeIndex;
    public SpringAnimation SwitchAnimation => _switchAnimation;

    public bool SetActiveWorkspace(int index)
    {
        if (index < 0 || index >= _workspaces.Count) return false;
        _activeIndex = index;
        _switchAnimation.SnapTo(index);
        OnWorkspaceChanged?.Invoke(index);
        return true;
    }

    public void SwitchTo(int index)
    {
        if (index < 0 || index >= _workspaces.Count) return;
        var oldIndex = _activeIndex;
        _activeIndex = index;
        _switchAnimation.Target = index;
        _switchAnimation.Value = oldIndex;
        _switchAnimation.Velocity = 0;
        OnWorkspaceChanging?.Invoke(oldIndex, index);
    }

    public void SwitchNext()
    {
        SwitchTo((_activeIndex + 1) % _workspaces.Count);
    }

    public void SwitchPrevious()
    {
        SwitchTo((_activeIndex - 1 + _workspaces.Count) % _workspaces.Count);
    }

    public void AddWindowToCurrent(CompositorWindow window)
    {
        ActiveWorkspace?.AddWindow(window);
    }

    public void MoveWindowToWorkspace(CompositorWindow window, int workspaceIndex)
    {
        foreach (var ws in _workspaces)
            ws.RemoveWindow(window);

        if (workspaceIndex >= 0 && workspaceIndex < _workspaces.Count)
            _workspaces[workspaceIndex].AddWindow(window);
    }

    public event Action<int>? OnWorkspaceChanged;
    public event Action<int, int>? OnWorkspaceChanging;

    public void Dispose()
    {
        _workspaces.Clear();
    }
}
