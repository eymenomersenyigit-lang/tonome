namespace Tonome.Framework.Input;

public class ShortcutManager
{
    private readonly Dictionary<string, Action> _shortcuts = new(StringComparer.OrdinalIgnoreCase);

    public void Register(string shortcut, Action action)
    {
        _shortcuts[shortcut] = action;
    }

    public bool Handle(string shortcut)
    {
        if (_shortcuts.TryGetValue(shortcut, out var action))
        {
            action();
            return true;
        }
        return false;
    }

    public void RegisterDefaultShortcuts()
    {
        Register("Super+R", () => OnRunCommand?.Invoke());
        Register("Super+Tab", () => OnDesktopSwitch?.Invoke());
        Register("Alt+Tab", () => OnWindowSwitch?.Invoke());
        Register("Super+D", () => OnShowDesktop?.Invoke());
        Register("Super+Q", () => OnCloseWindow?.Invoke());
        Register("Super+Space", () => OnAppLauncher?.Invoke());
        Register("Super+N", () => OnNotificationToggle?.Invoke());
        Register("Super+F", () => OnSearch?.Invoke(""));
    }

    public event Action? OnRunCommand;
    public event Action? OnDesktopSwitch;
    public event Action? OnWindowSwitch;
    public event Action? OnShowDesktop;
    public event Action? OnCloseWindow;
    public event Action? OnAppLauncher;
    public event Action? OnNotificationToggle;
    public event Action<string>? OnSearch;
}
