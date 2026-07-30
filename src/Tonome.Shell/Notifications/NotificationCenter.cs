using SkiaSharp;
using Tonome.Framework.Controls;
using Tonome.Framework.Types;
using Tonome.Framework.Animation;

namespace Tonome.Shell.Notifications;

public class Notification
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];
    public string Title { get; set; } = "";
    public string Message { get; set; } = "";
    public string AppName { get; set; } = "";
    public bool IsUrgent { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public Action? OnClick { get; set; }
}

public class NotificationCenter : Panel
{
    private readonly List<Notification> _notifications = new();
    private readonly List<NotificationBubble> _activeBubbles = new();
    private bool _isOpen;
    public int ScreenWidth { get; set; } = 1920;

    public NotificationCenter()
    {
        Width = 380;
        Height = 500;
        X = ScreenWidth - Width - 12;
        Y = 56;
        CornerRadius = 16;
        Visible = false;
    }

    public void ShowNotification(string title, string message, string app = "", bool urgent = false)
    {
        var notif = new Notification
        {
            Title = title,
            Message = message,
            AppName = app,
            IsUrgent = urgent
        };
        _notifications.Insert(0, notif);
        if (_notifications.Count > 50)
            _notifications.RemoveAt(_notifications.Count - 1);

        var bubble = new NotificationBubble(notif, ScreenWidth);
        _activeBubbles.Add(bubble);

        if (urgent)
        {
            Console.Beep();
        }
    }

    public void Toggle()
    {
        _isOpen = !_isOpen;
        Visible = _isOpen;
    }

    public void Dismiss(string id)
    {
        _notifications.RemoveAll(n => n.Id == id);
    }

    public override void Render(SKCanvas canvas, double delta)
    {
        foreach (var bubble in _activeBubbles.ToList())
        {
            bubble.Render(canvas, delta);
            if (bubble.IsExpired)
                _activeBubbles.Remove(bubble);
        }

        if (!_isOpen) return;

        var rect = new SKRect(X, Y, X + Width, Y + Height);
        using var glassPaint = new SKPaint
        {
            Color = new SKColor(25, 25, 35, 235),
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 8),
            IsAntialias = true
        };
        canvas.DrawRoundRect(rect, CornerRadius, CornerRadius, glassPaint);

        using var font = new SKFont(
            SKTypeface.FromFamilyName("Segoe UI", SKFontStyleWeight.SemiBold,
                SKFontStyleWidth.Normal, SKFontStyleSlant.Upright), 14);
        using var titlePaint = new SKPaint
        {
            Color = SKColors.White,
            IsAntialias = true
        };
        canvas.DrawText("Notifications", X + 16, Y + 28, SKTextAlign.Left, font, titlePaint);

        var itemY = Y + 48;
        foreach (var notif in _notifications.Take(8))
        {
            using var itemBg = new SKPaint
            {
                Color = notif.IsUrgent
                    ? new SKColor(180, 40, 40, 40)
                    : new SKColor(255, 255, 255, 8),
                IsAntialias = true
            };
            var itemRect = new SKRect(X + 8, itemY, X + Width - 8, itemY + 56);
            canvas.DrawRoundRect(itemRect, 8, 8, itemBg);

            using var titleFont = new SKFont(null, 12);
            using var nTitlePaint = new SKPaint
            {
                Color = new SKColor(255, 255, 255, (byte)(notif.IsUrgent ? 255 : 200)),
                IsAntialias = true
            };
            canvas.DrawText(notif.Title, X + 18, itemY + 18, SKTextAlign.Left, titleFont, nTitlePaint);

            using var msgFont = new SKFont(null, 11);
            using var msgPaint = new SKPaint
            {
                Color = new SKColor(200, 200, 220, 180),
                IsAntialias = true
            };
            var msg = notif.Message.Length > 40 ? notif.Message[..37] + "..." : notif.Message;
            canvas.DrawText(msg, X + 18, itemY + 36, SKTextAlign.Left, msgFont, msgPaint);

            using var timeFont = new SKFont(null, 9);
            using var timePaint = new SKPaint
            {
                Color = new SKColor(150, 150, 180, 150),
                IsAntialias = true
            };
            canvas.DrawText(notif.Timestamp.ToString("HH:mm"), X + Width - 50, itemY + 16,
                SKTextAlign.Left, timeFont, timePaint);

            itemY += 62;
        }
    }
}

public class NotificationBubble
{
    private readonly Notification _notification;
    private readonly SpringAnimation _slideIn = new(-100, 0);
    private readonly SpringAnimation _opacity = new(1, 1);
    private DateTime _startTime;
    private readonly int _screenWidth;
    private static readonly int DisplayDurationMs = 4000;

    public bool IsExpired => (DateTime.Now - _startTime).TotalMilliseconds > DisplayDurationMs;

    public NotificationBubble(Notification notification, int screenWidth)
    {
        _notification = notification;
        _screenWidth = screenWidth;
        _startTime = DateTime.Now;
    }

    public void Render(SKCanvas canvas, double delta)
    {
        _slideIn.Update(delta);
        _opacity.Update(delta);

        var bubbleW = 340;
        var bubbleH = 64;
        var bx = _screenWidth - bubbleW - 16 + _slideIn.Value;
        var by = 56;

        using var bgPaint = new SKPaint
        {
            Color = new SKColor(30, 30, 42, (byte)(230 * _opacity.Value)),
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 6),
            IsAntialias = true
        };
        var rect = new SKRect(bx, by, bx + bubbleW, by + bubbleH);
        canvas.DrawRoundRect(rect, 12, 12, bgPaint);

        using var font = new SKFont(null, 12);
        using var textPaint = new SKPaint
        {
            Color = new SKColor(255, 255, 255, (byte)(220 * _opacity.Value)),
            IsAntialias = true
        };
        canvas.DrawText(_notification.Title, bx + 12, by + 20, SKTextAlign.Left, font, textPaint);

        using var msgFont = new SKFont(null, 11);
        using var msgPaint = new SKPaint
        {
            Color = new SKColor(200, 200, 220, (byte)((int)(180 * _opacity.Value))),
            IsAntialias = true
        };
        var msg = _notification.Message.Length > 35
            ? _notification.Message[..32] + "..."
            : _notification.Message;
        canvas.DrawText(msg, bx + 12, by + 40, SKTextAlign.Left, msgFont, msgPaint);
    }
}
