using SkiaSharp;
using Tonome.Framework.Controls;
using Tonome.Framework.Types;
using Tonome.Framework.Animation;

namespace Tonome.Shell.Launcher;

public class AppLauncher : Panel
{
    public int ScreenWidth { get; set; } = 1920;
    public int ScreenHeight { get; set; } = 1080;
    public bool IsOpen { get; private set; }

    private readonly SpringAnimation _openAnim = new(0, 0);
    private readonly SpringAnimation _scaleAnim = new(0, 0);
    private string _searchText = "";
    private readonly List<LauncherAppItem> _allApps = new();
    private List<LauncherAppItem> _filteredApps = new();
    private int _selectedIndex;

    public AppLauncher()
    {
        Visible = false;
        Width = 600;
        Height = 700;
        CornerRadius = 24;
    }

    public void AddApp(string name, string category = "Other", Action? onClick = null)
    {
        var app = new LauncherAppItem
        {
            AppName = name,
            Category = category,
            OnActivate = onClick
        };
        _allApps.Add(app);
    }

    public void Open()
    {
        IsOpen = true;
        Visible = true;
        _openAnim.Target = 1;
        _scaleAnim.Target = 1;
        _searchText = "";
        _filteredApps = new List<LauncherAppItem>(_allApps);
        _selectedIndex = 0;
        X = (ScreenWidth - Width) / 2;
        Y = (ScreenHeight - Height) / 2;
    }

    public void Close()
    {
        IsOpen = false;
        _openAnim.Target = 0;
        _scaleAnim.Target = 0;
        Visible = false;
    }

    public void Toggle()
    {
        if (IsOpen) Close();
        else Open();
    }

    public void Search(string query)
    {
        _searchText = query;
        if (string.IsNullOrWhiteSpace(query))
        {
            _filteredApps = new List<LauncherAppItem>(_allApps);
        }
        else
        {
            _filteredApps = _allApps
                .Where(a => a.AppName.Contains(query, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
        _selectedIndex = 0;
    }

    public void SelectNext() => _selectedIndex = (_selectedIndex + 1) % Math.Max(1, _filteredApps.Count);
    public void SelectPrev() => _selectedIndex = (_selectedIndex - 1 + _filteredApps.Count) % Math.Max(1, _filteredApps.Count);

    public void ActivateSelected()
    {
        if (_selectedIndex >= 0 && _selectedIndex < _filteredApps.Count)
        {
            _filteredApps[_selectedIndex].OnActivate?.Invoke();
            Close();
        }
    }

    public override void Render(SKCanvas canvas, double delta)
    {
        _openAnim.Update(delta);
        _scaleAnim.Update(delta);

        if (!IsOpen && _openAnim.IsCompleted) return;

        using var bgPaint = new SKPaint
        {
            Color = new SKColor(0, 0, 0, (byte)(140 * _openAnim.Value))
        };
        canvas.DrawRect(0, 0, ScreenWidth, ScreenHeight, bgPaint);

        canvas.Save();
        var cx = X + Width / 2f;
        var cy = Y + Height / 2f;
        canvas.Scale(_scaleAnim.Value, _scaleAnim.Value, cx, cy);

        var rect = new SKRect(X, Y, X + Width, Y + Height);
        using var glassPaint = new SKPaint
        {
            Color = new SKColor(25, 25, 35, 240),
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 12),
            IsAntialias = true
        };
        canvas.DrawRoundRect(rect, CornerRadius, CornerRadius, glassPaint);

        using var borderPaint = new SKPaint
        {
            Color = new SKColor(255, 255, 255, 25),
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1
        };
        canvas.DrawRoundRect(rect, CornerRadius, CornerRadius, borderPaint);

        using var inputBg = new SKPaint
        {
            Color = new SKColor(255, 255, 255, 15),
            IsAntialias = true
        };
        var inputRect = new SKRect(X + 20, Y + 20, X + Width - 20, Y + 56);
        canvas.DrawRoundRect(inputRect, 10, 10, inputBg);

        using var searchFont = new SKFont(null, 15);
        using var searchPaint = new SKPaint
        {
            Color = string.IsNullOrEmpty(_searchText)
                ? new SKColor(255, 255, 255, 80)
                : new SKColor(255, 255, 255, 220),
            IsAntialias = true
        };
        var displayText = string.IsNullOrEmpty(_searchText) ? "Search apps..." : _searchText;
        canvas.DrawText(displayText, X + 32, Y + 44, SKTextAlign.Left, searchFont, searchPaint);

        var itemY = Y + 72;
        for (var i = 0; i < _filteredApps.Count && i < 10; i++)
        {
            var app = _filteredApps[i];
            var isSelected = i == _selectedIndex;

            if (isSelected)
            {
                using var selPaint = new SKPaint
                {
                    Color = new SKColor(0, 120, 212, 60),
                    IsAntialias = true
                };
                var selRect = new SKRect(X + 12, itemY - 2, X + Width - 12, itemY + 36);
                canvas.DrawRoundRect(selRect, 8, 8, selPaint);
            }

            using var iconFont = new SKFont(null, 14);
            using var iconPaint = new SKPaint
            {
                Color = new SKColor(255, 255, 255, 200),
                IsAntialias = true
            };
            canvas.DrawText(app.AppName, X + 24, itemY + 24, SKTextAlign.Left, iconFont, iconPaint);

            using var catFont = new SKFont(null, 10);
            using var catPaint = new SKPaint
            {
                Color = new SKColor(150, 150, 170, 200),
                IsAntialias = true
            };
            canvas.DrawText(app.Category, X + Width - 80, itemY + 24, SKTextAlign.Left, catFont, catPaint);

            itemY += 42;
        }

        canvas.Restore();
    }
}

public class LauncherAppItem
{
    public string AppName { get; set; } = "";
    public string Category { get; set; } = "Other";
    public Action? OnActivate { get; set; }
}
