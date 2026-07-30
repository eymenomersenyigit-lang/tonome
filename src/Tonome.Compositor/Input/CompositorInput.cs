namespace Tonome.Compositor.Input;

public class CompositorInput
{
    private readonly WindowManager _windowManager;
    private readonly Dictionary<string, Action> _keyBindings = new();
    private bool _altPressed;
    private bool _superPressed;

    public CompositorInput(WindowManager windowManager)
    {
        _windowManager = windowManager;
        RegisterDefaultBindings();
    }

    private void RegisterDefaultBindings()
    {
        Bind("Super+Tab", () => OnSuperTab?.Invoke());
        Bind("Alt+Tab", () => OnAltTab?.Invoke());
        Bind("Super+R", () => OnSuperR?.Invoke());
        Bind("Super+D", () => OnSuperD?.Invoke());
        Bind("Super+Q", () => OnSuperQ?.Invoke());
        Bind("Super+Space", () => OnSuperSpace?.Invoke());
        Bind("Super+L", () => OnSuperL?.Invoke());
        Bind("Super+Up", () => OnSuperUp?.Invoke());
        Bind("Super+Down", () => OnSuperDown?.Invoke());
        Bind("Super+Left", () => OnSuperLeft?.Invoke());
        Bind("Super+Right", () => OnSuperRight?.Invoke());
        Bind("Super+F", () => OnSuperF?.Invoke());
    }

    public void Bind(string shortcut, Action action)
    {
        _keyBindings[shortcut] = action;
    }

    public bool HandleKeyEvent(string key, bool pressed)
    {
        if (key == "Alt_L" || key == "Alt_R")
        {
            _altPressed = pressed;
            if (!pressed && !_altPressed) return false;
        }
        if (key == "Super_L" || key == "Super_R")
        {
            _superPressed = pressed;
            return false;
        }

        if (!pressed) return false;

        var combo = _superPressed ? $"Super+{FormatKey(key)}" :
                    _altPressed ? $"Alt+{FormatKey(key)}" :
                    FormatKey(key);

        if (_keyBindings.TryGetValue(combo, out var action))
        {
            action();
            return true;
        }

        if (_superPressed)
        {
            HandleSuperKey(key);
            return true;
        }

        return false;
    }

    private void HandleSuperKey(string key)
    {
        switch (key)
        {
            case "Tab":
                OnSuperTab?.Invoke();
                break;
            case "r":
            case "R":
                OnSuperR?.Invoke();
                break;
            case "d":
            case "D":
                OnSuperD?.Invoke();
                break;
            case "q":
            case "Q":
                OnSuperQ?.Invoke();
                break;
            case "space":
                OnSuperSpace?.Invoke();
                break;
        }
    }

    private static string FormatKey(string key)
    {
        return key switch
        {
            "Tab" => "Tab",
            "Return" or "KP_Enter" => "Enter",
            "Escape" => "Escape",
            "Left" => "Left",
            "Right" => "Right",
            "Up" => "Up",
            "Down" => "Down",
            "space" => "Space",
            _ when key.Length == 1 => key.ToUpper(),
            _ => key
        };
    }

    public bool HandleMouseMove(int x, int y)
    {
        var window = _windowManager.HitTest(x, y);
        OnMouseMove?.Invoke(x, y);
        return true;
    }

    public bool HandleMouseClick(int x, int y, int button)
    {
        var window = _windowManager.HitTest(x, y);
        if (window != null)
        {
            _windowManager.FocusWindow(window.Id);
            OnWindowClicked?.Invoke(window.Id);
        }
        OnMouseClick?.Invoke(x, y, button);
        return true;
    }

    public event Action? OnSuperTab;
    public event Action? OnAltTab;
    public event Action? OnSuperR;
    public event Action? OnSuperD;
    public event Action? OnSuperQ;
    public event Action? OnSuperSpace;
    public event Action? OnSuperL;
    public event Action? OnSuperUp;
    public event Action? OnSuperDown;
    public event Action? OnSuperLeft;
    public event Action? OnSuperRight;
    public event Action? OnSuperF;
    public event Action<int, int>? OnMouseMove;
    public event Action<int, int, int>? OnMouseClick;
    public event Action<int>? OnWindowClicked;
}
