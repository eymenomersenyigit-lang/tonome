using SkiaSharp;
using Tonome.Framework.Controls;
using Tonome.Framework.Types;

namespace Tonome.Shell.Panels;

public class SystemPanel : Panel
{
    public int DisplayWidth { get; set; } = 1920;

    private readonly Label _clockLabel;
    private readonly Label _dateLabel;
    private readonly List<TrayIcon> _trayIcons = new();

    public SystemPanel()
    {
        Height = 44;
        Width = DisplayWidth;
        CornerRadius = 0;

        _clockLabel = new Label
        {
            TextSize = 13,
            Bold = true,
            Width = 100,
            Height = 20,
            ForegroundColor = new Color(255, 255, 255)
        };

        _dateLabel = new Label
        {
            TextSize = 11,
            Width = 100,
            Height = 16,
            ForegroundColor = new Color(200, 200, 200)
        };

        AddChild(_clockLabel);
        AddChild(_dateLabel);

        _ = UpdateClockAsync();
    }

    private async Task UpdateClockAsync()
    {
        while (true)
        {
            var now = DateTime.Now;
            _clockLabel.Text = now.ToString("HH:mm:ss");
            _dateLabel.Text = now.ToString("ddd, dd MMM yyyy");
            _clockLabel.X = DisplayWidth - 120;
            _clockLabel.Y = 6;
            _dateLabel.X = DisplayWidth - 120;
            _dateLabel.Y = 26;
            await Task.Delay(1000);
        }
    }

    public void AddTrayIcon(string name, string icon)
    {
        var iconCtrl = new TrayIcon
        {
            Text = name[..1],
            Width = 28,
            Height = 28,
            CornerRadius = 6,
            BackgroundColor = new Color(255, 255, 255, 20),
            X = DisplayWidth - 160 - _trayIcons.Count * 34,
            Y = 8
        };
        _trayIcons.Add(iconCtrl);
        AddChild(iconCtrl);
    }

    public override void Render(SKCanvas canvas, double delta)
    {
        var rect = new SKRect(0, 0, Width, Height);
        using var bgPaint = new SKPaint
        {
            Color = new SKColor(15, 15, 25, 200),
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 4),
            IsAntialias = true
        };
        canvas.DrawRect(rect, bgPaint);

        using var linePaint = new SKPaint
        {
            Color = new SKColor(255, 255, 255, 15),
            StrokeWidth = 1
        };
        canvas.DrawLine(0, Height - 1, Width, Height - 1, linePaint);

        RenderAppMenu(canvas);

        foreach (var child in Children)
            if (child.Visible) child.Render(canvas, delta);
    }

    private void RenderAppMenu(SKCanvas canvas)
    {
        var menuItems = new[] { "Activities", "Terminal", "Files", "Browser" };
        var x = 12;
        foreach (var item in menuItems)
        {
            using var font = new SKFont(
                SKTypeface.FromFamilyName("Segoe UI", SKFontStyleWeight.Medium,
                    SKFontStyleWidth.Normal, SKFontStyleSlant.Upright), 12);
            using var paint = new SKPaint
            {
                Color = new SKColor(220, 220, 230),
                IsAntialias = true
            };
            canvas.DrawText(item, x, 28, SKTextAlign.Left, font, paint);
            x += (int)font.MeasureText(item) + 24;
        }
    }
}

public class TrayIcon : Control
{
    public string Text { get; set; } = "";

    public TrayIcon()
    {
        CornerRadius = 6;
    }

    public override void Render(SKCanvas canvas, double delta)
    {
        DrawBackground(canvas);
        if (!string.IsNullOrEmpty(Text))
        {
            using var font = new SKFont(null, 10);
            using var paint = new SKPaint
            {
                Color = new SKColor(255, 255, 255, 200),
                IsAntialias = true
            };
            canvas.DrawText(Text, AbsoluteX + Width / 2f, AbsoluteY + Height / 2f + 4,
                SKTextAlign.Center, font, paint);
        }
    }
}
