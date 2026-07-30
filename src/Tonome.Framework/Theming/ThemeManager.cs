namespace Tonome.Framework.Theming;

public class ThemeManager
{
    private Theme _current;
    private readonly Dictionary<string, Theme> _themes = new();

    public ThemeManager()
    {
        _current = Theme.Default;
        _themes["Default Glass"] = _current;
    }

    public Theme Current => _current;

    public void RegisterTheme(string name, Theme theme)
    {
        _themes[name] = theme;
    }

    public bool ApplyTheme(string name)
    {
        if (!_themes.TryGetValue(name, out var theme))
            return false;
        _current = theme;
        OnThemeChanged?.Invoke(name);
        return true;
    }

    public event Action<string>? OnThemeChanged;
}
