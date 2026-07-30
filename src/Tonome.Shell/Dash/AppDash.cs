using SkiaSharp;
using Tonome.Framework.Controls;
using Tonome.Framework.Types;
using Tonome.Framework.Animation;

namespace Tonome.Shell.Dash;

public class AppDash : Panel
{
    public int IconSize { get; set; } = 52;
    public int Spacing { get; set; } = 10;
    public int Padding { get; set; } = 14;
    public int ScreenWidth { get; set; } = 1920;
    public bool AutoHide { get; set; }

    private readonly SpringAnimation _hoverAnim = new(0, 0);
    private readonly List<DashAppItem> _items = new();
    private int _hoveredIndex = -1;

    public AppDash()
    {
        Height = IconSize + Padding * 2 + 10;
        CornerRadius = 18;
    }

    public void AddApp(string name, string icon = "", Action? onClick = null)
    {
        var item = new DashAppItem
        {
            AppName = name,
            Width = IconSize,
            Height = IconSize,
            CornerRadius = 16,
            BackgroundColor = new Color(255, 255, 255, 25),
            OnActivate = onClick
        };
        _items.Add(item);
        AddChild(item);
        Reflow();
    }

    public void RemoveApp(string name)
    {
        var item = _items.FirstOrDefault(i => i.AppName == name);
        if (item != null)
        {
            _items.Remove(item);
            RemoveChild(item);
            Reflow();
        }
    }

    private void Reflow()
    {
        var totalW = _items.Count * (IconSize + Spacing) - Spacing;
        X = (ScreenWidth - totalW) / 2;
        Width = totalW + Padding * 2;

        for (var i = 0; i < _items.Count; i++)
        {
            _items[i].X = Padding + i * (IconSize + Spacing);
            _items[i].Y = Padding;
            _items[i].Index = i;
        }
    }

    public override void Render(SKCanvas canvas, double delta)
    {
        _hoverAnim.Update(delta);

        var destY = ScreenWidth > 0 ? ScreenWidth - Height - 8 : 0;
        Y = (int)(destY + (1 - _hoverAnim.Value) * 20);

        DrawGlassBackground(canvas, 10f);

        foreach (var child in Children)
            if (child.Visible) child.Render(canvas, delta);
    }

    public void SetHovered(int index)
    {
        if (index != _hoveredIndex)
        {
            _hoveredIndex = index;
            _hoverAnim.Target = index >= 0 ? 1 : 0;
        }
    }
}

public class DashAppItem : Control
{
    public string AppName { get; set; } = "";
    public Action? OnActivate { get; set; }
    public int Index { get; set; }

    private readonly SpringAnimation _hoverScale = new(1, 1);
    private bool _isHovered;

    public override void Render(SKCanvas canvas, double delta)
    {
        _hoverScale.Update(delta);

        canvas.Save();
        var cx = AbsoluteX + Width / 2f;
        var cy = AbsoluteY + Height / 2f;
        canvas.Scale(_hoverScale.Value, _hoverScale.Value, cx, cy);

        using var bgPaint = new SKPaint
        {
            Color = _isHovered
                ? new SKColor(255, 255, 255, 40)
                : new SKColor(255, 255, 255, 20),
            IsAntialias = true
        };
        var rect = new SKRect(AbsoluteX, AbsoluteY, AbsoluteX + Width, AbsoluteY + Height);
        canvas.DrawRoundRect(rect, CornerRadius, CornerRadius, bgPaint);

        using var font = new SKFont(null, 18);
        using var textPaint = new SKPaint
        {
            Color = new SKColor(255, 255, 255, 220),
            IsAntialias = true
        };
        var initial = AppName.Length > 0 ? AppName[..1].ToUpper() : "?";
        canvas.DrawText(initial, cx, cy + 7, SKTextAlign.Center, font, textPaint);

        using var labelFont = new SKFont(null, 10);
        using var labelPaint = new SKPaint
        {
            Color = new SKColor(255, 255, 255, 180),
            IsAntialias = true
        };
        var label = AppName.Length > 8 ? AppName[..6] + ".." : AppName;
        canvas.DrawText(label, cx, AbsoluteY + Height + 14, SKTextAlign.Center, labelFont, labelPaint);

        canvas.Restore();
    }

    public override void OnMouseDown(int x, int y)
    {
        if (HitTest(x, y))
        {
            OnActivate?.Invoke();
        }
    }

    public override void OnMouseMove(int x, int y)
    {
        var wasHovered = _isHovered;
        _isHovered = HitTest(x, y);
        if (_isHovered != wasHovered)
        {
            _hoverScale.Target = _isHovered ? 1.15f : 1f;
        }
    }
}
